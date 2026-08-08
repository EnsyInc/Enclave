namespace EnsyInc.Enclave.Core.Models;

public enum LicenseStatus
{
    /// <summary>
    /// The license is scheduled to become active in the future.
    /// </summary>
    Scheduled = 0,

    /// <summary>
    /// The license is currently active and valid.
    /// </summary>
    Active = 1,

    /// <summary>
    /// The license has expired and is no longer valid.
    /// </summary>
    Expired = 2,

    /// <summary>
    /// The license has been suspended and is temporarily invalid.
    /// </summary>
    Suspended = 3,

    /// <summary>
    /// The license has been revoked and is permanently invalid.
    /// </summary>
    Revoked = 4,
}
