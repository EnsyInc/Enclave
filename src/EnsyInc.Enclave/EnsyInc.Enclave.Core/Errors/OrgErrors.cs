using EnsyNet.Core.Results;

namespace EnsyInc.Enclave.Core.Errors;

public sealed record OrgNotFoundError() : Error(ErrorCodes.OrgNotFoundError, "The org was not found.");
