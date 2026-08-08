namespace EnsyInc.Enclave.Core.Models;

public sealed record License : BaseModel
{
    public required Guid OrgId { get; init; }
    public required Guid ProductId { get; init; }
    public required DateTime Start { get; init; }
    public required DateTime End { get; init; }
    public required LicenseStatus Status { get; init; }
}
