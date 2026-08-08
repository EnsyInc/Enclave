using EnsyInc.Enclave.Core.Models;

using EnsyNet.DataAccess.Abstractions.Models;

namespace EnsyInc.Enclave.DataAccess.Models;

public sealed record OrgEntity : DbEntity
{
    public required string Name { get; init; }
    public required OrgStatus Status { get; init; }
    public Guid? PrimaryUserId { get; init; }
}
