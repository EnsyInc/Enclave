using JetBrains.Annotations;

namespace EnsyInc.Enclave.Api.Models.Apps;

[PublicAPI]
public sealed record RegisterAppRequest(
    string Name,
    string? Description);
