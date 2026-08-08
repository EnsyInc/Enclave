using EnsyNet.DataAccess.Abstractions.Models;

namespace EnsyInc.Enclave.DataAccess.Models;

public sealed record DbApp : DbEntity
{
    public required string Name { get; init; }
    public string? Description { get; init; }
}
