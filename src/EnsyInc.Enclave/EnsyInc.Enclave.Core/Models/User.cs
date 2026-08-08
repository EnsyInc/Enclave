namespace EnsyInc.Enclave.Core.Models;

public sealed record User : BaseModel
{
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required Guid OrgId { get; init; }
    public required UserStatus Status { get; init; }
    public required UserRole Role { get; init; }
}
