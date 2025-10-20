using JetBrains.Annotations;

namespace EnsyInc.Enclave.Api.Models.Apps;

[PublicAPI]
public sealed record UpdateAppRequest(
    string? Name,
    string? Description);
