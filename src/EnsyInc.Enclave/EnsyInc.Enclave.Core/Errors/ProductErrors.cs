using EnsyNet.Core.Results;

namespace EnsyInc.Enclave.Core.Errors;

public sealed record ProductNotFoundError() : Error(ErrorCodes.ProductNotFoundError, "The product was not found.");
