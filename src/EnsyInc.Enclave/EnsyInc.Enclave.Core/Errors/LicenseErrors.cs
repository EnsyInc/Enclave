using EnsyNet.Core.Results;

namespace EnsyInc.Enclave.Core.Errors;

public sealed record LicenseNotFoundError() : Error(ErrorCodes.LicenseNotFoundError, "The license was not found.");

public sealed record LicenseAlreadyExistsError(Guid ExistingLicenseId) : Error(ErrorCodes.LicenseAlreadyExistsError, "The org already has an active license for this product.");
