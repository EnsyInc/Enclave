namespace EnsyInc.Enclave.Core.Errors;

public static class ErrorCodes
{
    #region Generic Errors
    
    public const string UnexpectedError = "[UnexpectedError]";

    #endregion

    #region Products

    public const string ProductNotFoundError = "[ProductNotFoundError]";

    #endregion

    #region Orgs

    public const string OrgNotFoundError = "[OrgNotFoundError]";

    #endregion

    #region Users

    public const string UserNotFoundError = "[UserNotFoundError]";

    #endregion

    #region Licenses

    public const string LicenseNotFoundError = "[LicenseNotFoundError]";
    public const string LicenseAlreadyExistsError = "[LicenseAlreadyExistsError]";

    #endregion

    #region LicenseRequests

    public const string LicenseRequestNotFoundError = "[LicenseRequestNotFoundError]";
    public const string LicenseRequestNotPendingError = "[LicenseRequestNotPendingError]";
    public const string LicenseRequestStartDateRequiredError = "[LicenseRequestStartDateRequiredError]";
    public const string LicenseRequestInvalidDateRangeError = "[LicenseRequestInvalidDateRangeError]";

    #endregion
}
