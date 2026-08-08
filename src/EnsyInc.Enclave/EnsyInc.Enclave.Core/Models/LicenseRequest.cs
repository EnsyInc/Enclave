namespace EnsyInc.Enclave.Core.Models;

public sealed record LicenseRequest : BaseModel
{
    public required Guid OrgId { get; init; }
    public required Guid ProductId { get; init; }
    public required Guid UserId { get; init; }
    public Guid? ExistingLicenseId { get; init; }
    public string? RequestNotes { get; init; }
    public required LicenseRequestStatus Status { get; init; }
    public string? RejectionReason { get; init; }
}
