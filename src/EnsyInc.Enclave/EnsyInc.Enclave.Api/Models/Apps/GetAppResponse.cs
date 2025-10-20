using JetBrains.Annotations;

namespace EnsyInc.Enclave.Api.Models.Apps;

[PublicAPI]
public sealed record GetAppResponse(
    Guid Id,
    string Name,
    string? Description,
    DateTime CreatedAt,
    DateTime UpdatedAt);
