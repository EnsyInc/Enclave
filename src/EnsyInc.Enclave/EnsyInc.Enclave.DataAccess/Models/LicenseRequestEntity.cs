using EnsyInc.Enclave.Core.Models;

using EnsyNet.DataAccess.Abstractions.Models;

namespace EnsyInc.Enclave.DataAccess.Models;

public sealed record LicenseRequestEntity : DbEntity
{
    public required Guid OrgId { get; init; }
    public required Guid ProductId { get; init; }
    public required Guid UserId { get; init; }
    public Guid? ExistingLicenseId { get; init; }
    public string? RequestNotes { get; init; }
    public required LicenseRequestStatus Status { get; init; }
    public string? RejectionReason { get; init; }
}
