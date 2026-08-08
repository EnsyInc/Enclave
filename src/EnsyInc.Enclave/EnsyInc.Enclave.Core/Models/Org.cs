namespace EnsyInc.Enclave.Core.Models;

public sealed record Org : BaseModel
{
    public required string Name { get; init; }
    public required OrgStatus Status { get; init; }
    public Guid? PrimaryUserId { get; init; }
}
