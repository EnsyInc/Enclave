using JetBrains.Annotations;

namespace EnsyInc.Enclave.Api.Models.Apps;

[PublicAPI]
public sealed record GetAppsResponse(IEnumerable<GetAppResponse> Apps);
