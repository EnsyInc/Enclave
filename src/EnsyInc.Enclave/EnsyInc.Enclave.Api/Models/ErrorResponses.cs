namespace EnsyInc.Enclave.Api.Models;

internal static class ErrorResponses
{
    public static readonly ErrorResponse ProductNotFoundError = new("ProductNotFound", "The requested product was not found.", []);

    public static readonly ErrorResponse OrgNotFoundError = new("OrgNotFound", "The requested org was not found.", []);

    public static readonly ErrorResponse UserNotFoundError = new("UserNotFound", "The requested user was not found.", []);

    public static readonly ErrorResponse LicenseNotFoundError = new("LicenseNotFound", "The requested license was not found.", []);

    public static readonly ErrorResponse LicenseRequestNotFoundError = new("LicenseRequestNotFound", "The requested license request was not found.", []);

    public static readonly ErrorResponse LicenseRequestNotPendingError = new("LicenseRequestNotPending", "The license request has already been reviewed.", []);

    public static readonly ErrorResponse LicenseRequestStartDateRequiredError = new("LicenseRequestStartDateRequired", "A start date is required to approve a new-license request.", []);

    public static readonly ErrorResponse LicenseRequestInvalidDateRangeError = new("LicenseRequestInvalidDateRange", "The new expiry date must be after the license's start date.", []);

    public static readonly ErrorResponse UnexpectedError = new("UnexpectedError", "An unexpected error occurred.", []);

    public static ErrorResponse ValidationError(Dictionary<string, string> parameters)
        => new("ValidationError", "One or more fields failed validation.", parameters);

    public static ErrorResponse LicenseAlreadyExistsError(Guid existingLicenseId)
        => new("LicenseAlreadyExists", "The org already has an active license for this product.", new Dictionary<string, string> { ["ExistingLicenseId"] = existingLicenseId.ToString() });
}
