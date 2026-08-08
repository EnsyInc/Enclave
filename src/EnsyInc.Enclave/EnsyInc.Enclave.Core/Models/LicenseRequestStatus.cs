namespace EnsyInc.Enclave.Core.Models;

public enum LicenseRequestStatus
{
    /// <summary>
    /// The license request is pending and has not yet been reviewed or approved.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// The license request has been reviewed and approved.
    /// </summary>
    Approved = 1,

    /// <summary>
    /// The license request has been reviewed and rejected.
    /// </summary>
    Rejected = 2,
}
