using EnsyNet.Core.Results;

namespace EnsyInc.Enclave.Core.Errors;

public sealed record LicenseRequestNotFoundError() : Error(ErrorCodes.LicenseRequestNotFoundError, "The license request was not found.");

public sealed record LicenseRequestNotPendingError() : Error(ErrorCodes.LicenseRequestNotPendingError, "The license request has already been reviewed.");

public sealed record LicenseRequestStartDateRequiredError() : Error(ErrorCodes.LicenseRequestStartDateRequiredError, "A start date is required to approve a new-license request.");

public sealed record LicenseRequestInvalidDateRangeError() : Error(ErrorCodes.LicenseRequestInvalidDateRangeError, "The new expiry date must be after the license's start date.");
