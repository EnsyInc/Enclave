namespace EnsyInc.Enclave.Core.Models;

public abstract record BaseModel
{
    public Guid Id { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastUpdatedAt { get; init; }
    public DateTime? DeletedAt { get; init; }
}
