using EnsyNet.Core.Results;

namespace EnsyInc.Enclave.Core.Errors;

public sealed record UnexpectedError() : Error(ErrorCodes.UnexpectedError, "An unexpected error occurred.");
