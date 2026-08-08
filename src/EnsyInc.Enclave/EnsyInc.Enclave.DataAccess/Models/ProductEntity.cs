using EnsyInc.Enclave.Core.Models;

using EnsyNet.DataAccess.Abstractions.Models;

namespace EnsyInc.Enclave.DataAccess.Models;

public sealed record ProductEntity : DbEntity
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required ProductStatus Status { get; init; }
}
