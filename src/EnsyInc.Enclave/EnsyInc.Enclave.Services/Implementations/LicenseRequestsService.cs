using EnsyInc.Enclave.Core.Errors;
using EnsyInc.Enclave.Core.Models;
using EnsyInc.Enclave.DataAccess.Abstractions;
using EnsyInc.Enclave.DataAccess.Mappers;
using EnsyInc.Enclave.DataAccess.Models;
using EnsyInc.Enclave.Services.Abstractions;

using EnsyNet.Core.Results;
using EnsyNet.DataAccess.Abstractions.Errors;

namespace EnsyInc.Enclave.Services.Implementations;

internal sealed class LicenseRequestsService(ILicenseRequestRepo licenseRequestRepo, ILicenseRepo licenseRepo) : ILicenseRequestsService
{
    public async Task<Result<IEnumerable<LicenseRequest>>> ListLicenseRequests(Guid? orgId, Guid? productId, LicenseRequestStatus? status, CancellationToken ct)
    {
        var result = await licenseRequestRepo.GetManyByExpression(
            r => (orgId == null || r.OrgId == orgId) && (productId == null || r.ProductId == productId) && (status == null || r.Status == status),
            ct);

        return result.HasError
            ? Result.FromError<IEnumerable<LicenseRequest>>(new UnexpectedError())
            : Result.Ok(result.Data.Select(e => e.ToCoreModel()));
    }

    public async Task<Result<LicenseRequest?>> GetLicenseRequest(Guid id, CancellationToken ct)
    {
        var result = await licenseRequestRepo.GetById(id, ct);

        if (result.HasError)
        {
            return result.Error switch
            {
                EntityNotFoundError<LicenseRequestEntity> => Result.FromError<LicenseRequest?>(new LicenseRequestNotFoundError()),
                _ => Result.FromError<LicenseRequest?>(new UnexpectedError()),
            };
        }

        return Result.Ok<LicenseRequest?>(result.Data.ToCoreModel());
    }

    public async Task<Result<LicenseRequest>> ApproveLicenseRequest(Guid id, DateTime? start, DateTime end, CancellationToken ct)
    {
        var existing = await GetLicenseRequest(id, ct);
        if (existing.HasError)
        {
            return Result.FromError<LicenseRequest>(existing.Error);
        }

        if (existing.Data.Status != LicenseRequestStatus.Pending)
        {
            return Result.FromError<LicenseRequest>(new LicenseRequestNotPendingError());
        }

        var licenseResult = existing.Data.ExistingLicenseId is null
            ? await ApproveAsNewLicense(existing.Data, start, end, ct)
            : await ApproveAsRenewal(existing.Data.ExistingLicenseId.Value, end, ct);

        if (licenseResult.HasError)
        {
            return Result.FromError<LicenseRequest>(licenseResult.Error);
        }

        // Best-effort, non-atomic across the License and LicenseRequest repos (BaseRepository
        // exposes no cross-repository transaction, same documented limitation as
        // UsersService.InviteUsers): if this update fails after the License write above already
        // succeeded, the request is left Pending with a License already granted/renewed under it.
        var updateResult = await licenseRequestRepo.Update(id, updates => updates.AddUpdate(r => r.Status, _ => LicenseRequestStatus.Approved), ct);

        if (updateResult.HasError)
        {
            return updateResult.Error switch
            {
                UpdateOperationFailedError => Result.FromError<LicenseRequest>(new LicenseRequestNotFoundError()),
                _ => Result.FromError<LicenseRequest>(new UnexpectedError()),
            };
        }

        return Result.Ok(existing.Data with { Status = LicenseRequestStatus.Approved, UpdatedAt = DateTime.UtcNow });
    }

    public async Task<Result<LicenseRequest>> RejectLicenseRequest(Guid id, string? reason, CancellationToken ct)
    {
        var existing = await GetLicenseRequest(id, ct);
        if (existing.HasError)
        {
            return Result.FromError<LicenseRequest>(existing.Error);
        }

        if (existing.Data.Status != LicenseRequestStatus.Pending)
        {
            return Result.FromError<LicenseRequest>(new LicenseRequestNotPendingError());
        }

        var updateResult = await licenseRequestRepo.Update(id, updates =>
        {
            updates.AddUpdate(r => r.Status, _ => LicenseRequestStatus.Rejected);
            updates.AddUpdate(r => r.RejectionReason, _ => reason);
        }, ct);

        if (updateResult.HasError)
        {
            return updateResult.Error switch
            {
                UpdateOperationFailedError => Result.FromError<LicenseRequest>(new LicenseRequestNotFoundError()),
                _ => Result.FromError<LicenseRequest>(new UnexpectedError()),
            };
        }

        return Result.Ok(existing.Data with { Status = LicenseRequestStatus.Rejected, RejectionReason = reason, UpdatedAt = DateTime.UtcNow });
    }

    private async Task<Result> ApproveAsNewLicense(LicenseRequest request, DateTime? start, DateTime end, CancellationToken ct)
    {
        if (start is null)
        {
            return Result.FromError(new LicenseRequestStartDateRequiredError());
        }

        var license = new License
        {
            OrgId = request.OrgId,
            ProductId = request.ProductId,
            Start = start.Value,
            End = end,
            Status = ComputeInitialStatus(start.Value),
        };

        var insertResult = await licenseRepo.Insert(license.ToEntityModel(), ct);

        if (insertResult.HasError)
        {
            if (insertResult.Error is UniqueConstraintViolationError)
            {
                var alreadyExists = await BuildAlreadyExistsError<bool>(request.OrgId, request.ProductId, ct);
                return alreadyExists.HasError ? Result.FromError(alreadyExists.Error) : Result.Ok();
            }

            return Result.FromError(new UnexpectedError());
        }

        return Result.Ok();
    }

    private static LicenseStatus ComputeInitialStatus(DateTime start)
        => start > DateTime.UtcNow ? LicenseStatus.Scheduled : LicenseStatus.Active;

    /// <summary>
    /// Builds a <see cref="LicenseAlreadyExistsError"/> carrying the id of the conflicting license, so callers
    /// (e.g. the frontend) can link/redirect straight to it instead of just knowing a conflict occurred.
    /// </summary>
    private async Task<Result<T>> BuildAlreadyExistsError<T>(Guid orgId, Guid productId, CancellationToken ct)
    {
        var existingLicense = await licenseRepo.GetByExpression(l => l.OrgId == orgId && l.ProductId == productId, ct);

        return existingLicense.HasError
            ? Result.FromError<T>(new UnexpectedError())
            : Result.FromError<T>(new LicenseAlreadyExistsError(existingLicense.Data.Id));
    }

    private async Task<Result> ApproveAsRenewal(Guid existingLicenseId, DateTime end, CancellationToken ct)
    {
        var existingLicenseResult = await licenseRepo.GetById(existingLicenseId, ct);

        if (existingLicenseResult.HasError)
        {
            return existingLicenseResult.Error switch
            {
                EntityNotFoundError<LicenseEntity> => Result.FromError(new LicenseNotFoundError()),
                _ => Result.FromError(new UnexpectedError()),
            };
        }

        if (end <= existingLicenseResult.Data.Start)
        {
            return Result.FromError(new LicenseRequestInvalidDateRangeError());
        }

        var updateResult = await licenseRepo.Update(existingLicenseId, updates =>
        {
            updates.AddUpdate(l => l.End, _ => end);
            updates.AddUpdate(l => l.Status, _ => LicenseStatus.Active);
        }, ct);

        if (updateResult.HasError)
        {
            return updateResult.Error switch
            {
                UpdateOperationFailedError => Result.FromError(new LicenseNotFoundError()),
                _ => Result.FromError(new UnexpectedError()),
            };
        }

        return Result.Ok();
    }
}
