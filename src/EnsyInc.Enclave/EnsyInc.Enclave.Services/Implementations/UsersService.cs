using EnsyInc.Enclave.Core.Errors;
using EnsyInc.Enclave.Core.Models;
using EnsyInc.Enclave.DataAccess.Abstractions;
using EnsyInc.Enclave.DataAccess.Mappers;
using EnsyInc.Enclave.DataAccess.Models;
using EnsyInc.Enclave.Services.Abstractions;

using EnsyNet.Core.Results;
using EnsyNet.DataAccess.Abstractions.Errors;

namespace EnsyInc.Enclave.Services.Implementations;

internal sealed class UsersService(IUserRepo userRepo, IOrgRepo orgRepo) : IUsersService
{
    public async Task<Result<IEnumerable<User>>> ListUsers(Guid orgId, CancellationToken ct)
    {
        var orgResult = await EnsureOrgExists<IEnumerable<User>>(orgId, ct);
        if (orgResult is not null)
        {
            return orgResult;
        }

        var result = await userRepo.GetManyByExpression(u => u.OrgId == orgId, ct);
        return result.HasError
            ? Result.FromError<IEnumerable<User>>(new UnexpectedError())
            : Result.Ok(result.Data.Select(e => e.ToCoreModel()));
    }

    public async Task<Result<User?>> GetUser(Guid orgId, Guid id, CancellationToken ct)
    {
        var orgResult = await EnsureOrgExists<User?>(orgId, ct);
        if (orgResult is not null)
        {
            return orgResult;
        }

        var result = await userRepo.GetById(id, ct);

        if (result.HasError)
        {
            return result.Error switch
            {
                EntityNotFoundError<UserEntity> => Result.FromError<User?>(new UserNotFoundError()),
                _ => Result.FromError<User?>(new UnexpectedError()),
            };
        }

        return result.Data.OrgId != orgId
            ? Result.FromError<User?>(new UserNotFoundError())
            : Result.Ok<User?>(result.Data.ToCoreModel());
    }

    public async Task<Result<User>> InviteUser(Guid orgId, User user, CancellationToken ct)
    {
        var orgResult = await EnsureOrgExists<User>(orgId, ct);
        if (orgResult is not null)
        {
            return orgResult;
        }

        var result = await userRepo.Insert(user.ToEntityModel(), ct);

        if (result.HasError)
        {
            return result.Error switch
            {
                EntityNotFoundError<UserEntity> => Result.FromError<User>(new UserNotFoundError()),
                _ => Result.FromError<User>(new UnexpectedError()),
            };
        }

        return Result.Ok(result.Data.ToCoreModel());
    }

    public async Task<Result<IEnumerable<User>>> InviteUsers(Guid orgId, IEnumerable<User> users, CancellationToken ct)
    {
        var orgResult = await EnsureOrgExists<IEnumerable<User>>(orgId, ct);
        if (orgResult is not null)
        {
            return orgResult;
        }

        var invited = new List<User>();

        // Best-effort batch: no cross-repository transaction is available, so the first failure
        // stops the batch without rolling back users already inserted in this loop.
        foreach (var user in users)
        {
            var result = await userRepo.Insert(user.ToEntityModel(), ct);
            if (result.HasError)
            {
                return Result.FromError<IEnumerable<User>>(new UnexpectedError());
            }

            invited.Add(result.Data.ToCoreModel());
        }

        return Result.Ok<IEnumerable<User>>(invited);
    }

    public async Task<Result<User>> UpdateUser(Guid orgId, Guid id, string name, UserRole role, CancellationToken ct)
    {
        var orgResult = await EnsureOrgExists<User>(orgId, ct);
        if (orgResult is not null)
        {
            return orgResult;
        }

        var existing = await GetUser(orgId, id, ct);
        if (existing.HasError)
        {
            return Result.FromError<User>(existing.Error);
        }

        var updateResult = await userRepo.Update(id, updates =>
        {
            updates.AddUpdate(u => u.Name, _ => name);
            updates.AddUpdate(u => u.Role, _ => role);
        }, ct);

        if (updateResult.HasError)
        {
            return updateResult.Error switch
            {
                UpdateOperationFailedError => Result.FromError<User>(new UserNotFoundError()),
                _ => Result.FromError<User>(new UnexpectedError()),
            };
        }

        return Result.Ok(existing.Data with { Name = name, Role = role, UpdatedAt = DateTime.UtcNow });
    }

    public async Task<Result<User>> DeactivateUser(Guid orgId, Guid id, CancellationToken ct)
        => await SetStatus(orgId, id, UserStatus.Deactivated, ct);

    public async Task<Result<User>> ReactivateUser(Guid orgId, Guid id, CancellationToken ct)
        => await SetStatus(orgId, id, UserStatus.Active, ct);

    public async Task<Result<bool>> SoftDeleteUser(Guid orgId, Guid id, CancellationToken ct)
    {
        var orgResult = await EnsureOrgExists<bool>(orgId, ct);
        if (orgResult is not null)
        {
            return orgResult;
        }

        var existing = await GetUser(orgId, id, ct);
        if (existing.HasError)
        {
            return Result.Ok(true);
        }

        var result = await userRepo.SoftDelete(id, ct);

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

    private async Task<Result<User>> SetStatus(Guid orgId, Guid id, UserStatus status, CancellationToken ct)
    {
        var orgResult = await EnsureOrgExists<User>(orgId, ct);
        if (orgResult is not null)
        {
            return orgResult;
        }

        var existing = await GetUser(orgId, id, ct);
        if (existing.HasError)
        {
            return Result.FromError<User>(existing.Error);
        }

        var updateResult = await userRepo.Update(id, updates => updates.AddUpdate(u => u.Status, _ => status), ct);

        if (updateResult.HasError)
        {
            return updateResult.Error switch
            {
                UpdateOperationFailedError => Result.FromError<User>(new UserNotFoundError()),
                _ => Result.FromError<User>(new UnexpectedError()),
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
}
