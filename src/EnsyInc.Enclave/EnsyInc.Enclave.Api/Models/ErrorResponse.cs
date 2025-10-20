using JetBrains.Annotations;

namespace EnsyInc.Enclave.Api.Models;

[PublicAPI]
public sealed record ErrorResponse(
    string ErrorCode, 
    string ErrorMessage, 
    Dictionary<string, string> Parameters);
