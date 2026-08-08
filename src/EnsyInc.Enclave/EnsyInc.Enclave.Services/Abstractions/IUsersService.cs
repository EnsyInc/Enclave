using EnsyInc.Enclave.Core.Models;

using EnsyNet.Core.Results;

namespace EnsyInc.Enclave.Services.Abstractions;

public interface IUsersService
{
    public Task<Result<IEnumerable<User>>> ListUsers(Guid orgId, CancellationToken ct);
    public Task<Result<User?>> GetUser(Guid orgId, Guid id, CancellationToken ct);

    public Task<Result<User>> InviteUser(Guid orgId, User user, CancellationToken ct);
    public Task<Result<IEnumerable<User>>> InviteUsers(Guid orgId, IEnumerable<User> users, CancellationToken ct);

    public Task<Result<User>> UpdateUser(Guid orgId, Guid id, string name, UserRole role, CancellationToken ct);
    public Task<Result<User>> DeactivateUser(Guid orgId, Guid id, CancellationToken ct);
    public Task<Result<User>> ReactivateUser(Guid orgId, Guid id, CancellationToken ct);

    public Task<Result<bool>> SoftDeleteUser(Guid orgId, Guid id, CancellationToken ct);
}
