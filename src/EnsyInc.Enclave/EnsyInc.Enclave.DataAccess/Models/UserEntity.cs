using EnsyInc.Enclave.Core.Models;

using EnsyNet.DataAccess.Abstractions.Models;

namespace EnsyInc.Enclave.DataAccess.Models;

public sealed record UserEntity : DbEntity
{
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required Guid OrgId { get; init; }
    public required UserStatus Status { get; init; }
    public required UserRole Role { get; init; }
}
