using EnsyInc.Enclave.Core.Models;

using EnsyNet.DataAccess.Abstractions.Models;

namespace EnsyInc.Enclave.DataAccess.Models;

public sealed record LicenseEntity : DbEntity
{
    public required Guid OrgId { get; init; }
    public required Guid ProductId { get; init; }
    public required DateTime Start { get; init; }
    public required DateTime End { get; init; }
    public required LicenseStatus Status { get; init; }
}
