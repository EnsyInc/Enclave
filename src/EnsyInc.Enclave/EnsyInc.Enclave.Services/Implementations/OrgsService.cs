using EnsyInc.Enclave.Core.Errors;
using EnsyInc.Enclave.Core.Models;
using EnsyInc.Enclave.DataAccess.Abstractions;
using EnsyInc.Enclave.DataAccess.Mappers;
using EnsyInc.Enclave.DataAccess.Models;
using EnsyInc.Enclave.Services.Abstractions;

using EnsyNet.Core.Results;
using EnsyNet.DataAccess.Abstractions.Errors;

namespace EnsyInc.Enclave.Services.Implementations;

internal sealed class OrgsService(IOrgRepo orgRepo) : IOrgsService
{
    public async Task<Result<IEnumerable<Org>>> ListOrgs(CancellationToken ct)
    {
        var result = await orgRepo.GetAll(ct);
        return result.HasError
            ? Result.FromError<IEnumerable<Org>>(new UnexpectedError())
            : Result.Ok(result.Data.Select(e => e.ToCoreModel()));
    }

    public async Task<Result<IEnumerable<Org>>> ListOrgsByName(string name, CancellationToken ct)
    {
        // string.Contains(string, StringComparison) can't be translated by EF Core to SQL Server;
        // plain Contains() translates to LIKE and is already case-insensitive under SQL Server's
        // default (case-insensitive) collation.
        var result = await orgRepo.GetManyByExpression(o => o.Name.Contains(name), ct);
        return result.HasError
            ? Result.FromError<IEnumerable<Org>>(new UnexpectedError())
            : Result.Ok(result.Data.Select(e => e.ToCoreModel()));
    }

    public async Task<Result<Org?>> GetOrg(Guid id, CancellationToken ct)
    {
        var result = await orgRepo.GetById(id, ct);

        if (result.HasError)
        {
            return result.Error switch
            {
                EntityNotFoundError<OrgEntity> => Result.FromError<Org?>(new OrgNotFoundError()),
                _ => Result.FromError<Org?>(new UnexpectedError()),
            };
        }

        return Result.Ok<Org?>(result.Data.ToCoreModel());
    }

    public async Task<Result<Org>> CreateOrg(Org org)
    {
        var result = await orgRepo.Insert(org.ToEntityModel(), CancellationToken.None);

        if (result.HasError)
        {
            return result.Error switch
            {
                EntityNotFoundError<OrgEntity> => Result.FromError<Org>(new OrgNotFoundError()),
                _ => Result.FromError<Org>(new UnexpectedError()),
            };
        }

        return Result.Ok(result.Data.ToCoreModel());
    }

    public async Task<Result<Org>> UpdateOrg(Org org, CancellationToken ct)
    {
        var existing = await GetOrg(org.Id, ct);
        if (existing.HasError)
        {
            return Result.FromError<Org>(existing.Error);
        }

        var updateResult = await orgRepo.Update(org.Id, updates => updates.AddUpdate(o => o.Name, _ => org.Name), ct);

        if (updateResult.HasError)
        {
            return updateResult.Error switch
            {
                UpdateOperationFailedError => Result.FromError<Org>(new OrgNotFoundError()),
                _ => Result.FromError<Org>(new UnexpectedError()),
            };
        }

        // The update already told us it succeeded, so the new state is known without re-querying:
        // apply the same field change to the entity fetched above instead of fetching again.
        return Result.Ok(existing.Data with { Name = org.Name, UpdatedAt = DateTime.UtcNow });
    }

    public async Task<Result<Org>> DeactivateOrg(Guid id, CancellationToken ct)
        => await SetStatus(id, OrgStatus.Deactivated, ct);

    public async Task<Result<Org>> ReactivateOrg(Guid id, CancellationToken ct)
        => await SetStatus(id, OrgStatus.Active, ct);

    public async Task<Result<bool>> SoftDeleteOrg(Guid id, CancellationToken ct)
    {
        var result = await orgRepo.SoftDelete(id, ct);

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

    private async Task<Result<Org>> SetStatus(Guid id, OrgStatus status, CancellationToken ct)
    {
        var existing = await GetOrg(id, ct);
        if (existing.HasError)
        {
            return Result.FromError<Org>(existing.Error);
        }

        var updateResult = await orgRepo.Update(id, updates => updates.AddUpdate(o => o.Status, _ => status), ct);

        if (updateResult.HasError)
        {
            return updateResult.Error switch
            {
                UpdateOperationFailedError => Result.FromError<Org>(new OrgNotFoundError()),
                _ => Result.FromError<Org>(new UnexpectedError()),
            };
        }

        return Result.Ok(existing.Data with { Status = status, UpdatedAt = DateTime.UtcNow });
    }
}
