namespace EnsyInc.Enclave.Core.Models;

public sealed record Product : BaseModel
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required ProductStatus Status { get; init; }
}
