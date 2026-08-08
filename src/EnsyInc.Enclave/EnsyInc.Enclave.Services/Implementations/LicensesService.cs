using EnsyInc.Enclave.Core.Errors;
using EnsyInc.Enclave.Core.Models;
using EnsyInc.Enclave.DataAccess.Abstractions;
using EnsyInc.Enclave.DataAccess.Mappers;
using EnsyInc.Enclave.DataAccess.Models;
using EnsyInc.Enclave.Services.Abstractions;

using EnsyNet.Core.Results;
using EnsyNet.DataAccess.Abstractions.Errors;

namespace EnsyInc.Enclave.Services.Implementations;

internal sealed class LicensesService(ILicenseRepo licenseRepo, IOrgRepo orgRepo, IProductRepo productRepo) : ILicensesService
{
    public async Task<Result<IEnumerable<License>>> ListLicenses(Guid? orgId, Guid? productId, LicenseStatus? status, CancellationToken ct)
    {
        var result = await licenseRepo.GetManyByExpression(
            l => (orgId == null || l.OrgId == orgId) && (productId == null || l.ProductId == productId) && (status == null || l.Status == status),
            ct);

        return result.HasError
            ? Result.FromError<IEnumerable<License>>(new UnexpectedError())
            : Result.Ok(result.Data.Select(e => e.ToCoreModel()));
    }

    public async Task<Result<License?>> GetLicense(Guid id, CancellationToken ct)
    {
        var result = await licenseRepo.GetById(id, ct);

        if (result.HasError)
        {
            return result.Error switch
            {
                EntityNotFoundError<LicenseEntity> => Result.FromError<License?>(new LicenseNotFoundError()),
                _ => Result.FromError<License?>(new UnexpectedError()),
            };
        }

        return Result.Ok<License?>(result.Data.ToCoreModel());
    }

    public async Task<Result<License>> GrantLicense(Guid orgId, Guid productId, DateTime start, DateTime end, CancellationToken ct)
    {
        var orgResult = await EnsureOrgExists<License>(orgId, ct);
        if (orgResult is not null)
        {
            return orgResult;
        }

        var productResult = await EnsureProductExists<License>(productId, ct);
        if (productResult is not null)
        {
            return productResult;
        }

        var license = new License
        {
            OrgId = orgId,
            ProductId = productId,
            Start = start,
            End = end,
            Status = ComputeInitialStatus(start),
        };

        var result = await licenseRepo.Insert(license.ToEntityModel(), ct);

        if (result.HasError)
        {
            return result.Error switch
            {
                UniqueConstraintViolationError => await BuildAlreadyExistsError<License>(orgId, productId, ct),
                _ => Result.FromError<License>(new UnexpectedError()),
            };
        }

        return Result.Ok(result.Data.ToCoreModel());
    }

    public async Task<Result<License>> UpdateLicenseDates(Guid id, DateTime start, DateTime end, CancellationToken ct)
    {
        var existing = await GetLicense(id, ct);
        if (existing.HasError)
        {
            return Result.FromError<License>(existing.Error);
        }

        var updateResult = await licenseRepo.Update(id, updates =>
        {
            updates.AddUpdate(l => l.Start, _ => start);
            updates.AddUpdate(l => l.End, _ => end);
        }, ct);

        if (updateResult.HasError)
        {
            return updateResult.Error switch
            {
                UpdateOperationFailedError => Result.FromError<License>(new LicenseNotFoundError()),
                _ => Result.FromError<License>(new UnexpectedError()),
            };
        }

        return Result.Ok(existing.Data with { Start = start, End = end, UpdatedAt = DateTime.UtcNow });
    }

    public async Task<Result<License>> SuspendLicense(Guid id, CancellationToken ct)
        => await SetStatus(id, LicenseStatus.Suspended, ct);

    public async Task<Result<License>> RevokeLicense(Guid id, CancellationToken ct)
        => await SetStatus(id, LicenseStatus.Revoked, ct);

    public async Task<Result<bool>> SoftDeleteLicense(Guid id, CancellationToken ct)
    {
        var result = await licenseRepo.SoftDelete(id, ct);

        if (result.HasError)
        {
            return result.Error switch
            {
                DeleteOperationFailedError => Result.Ok(true),
                _ => Result.FromError<bool>(new UnexpectedError()),
            };
        }

        return Result.Ok(true);
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

    private async Task<Result<License>> SetStatus(Guid id, LicenseStatus status, CancellationToken ct)
    {
        var existing = await GetLicense(id, ct);
        if (existing.HasError)
        {
            return Result.FromError<License>(existing.Error);
        }

        var updateResult = await licenseRepo.Update(id, updates => updates.AddUpdate(l => l.Status, _ => status), ct);

        if (updateResult.HasError)
        {
            return updateResult.Error switch
            {
                UpdateOperationFailedError => Result.FromError<License>(new LicenseNotFoundError()),
                _ => Result.FromError<License>(new UnexpectedError()),
            };
        }

        return Result.Ok(existing.Data with { Status = status, UpdatedAt = DateTime.UtcNow });
    }

    private async Task<Result<T>?> EnsureOrgExists<T>(Guid orgId, CancellationToken ct)
    {
        var result = await orgRepo.GetById(orgId, ct);

        if (!result.HasError)
        {
            return null;
        }

        return result.Error switch
        {
            EntityNotFoundError<OrgEntity> => Result.FromError<T>(new OrgNotFoundError()),
            _ => Result.FromError<T>(new UnexpectedError()),
        };
    }

    private async Task<Result<T>?> EnsureProductExists<T>(Guid productId, CancellationToken ct)
    {
        var result = await productRepo.GetById(productId, ct);

        if (!result.HasError)
        {
            return null;
        }

        return result.Error switch
        {
            EntityNotFoundError<ProductEntity> => Result.FromError<T>(new ProductNotFoundError()),
            _ => Result.FromError<T>(new UnexpectedError()),
        };
    }
}
