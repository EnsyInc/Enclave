namespace EnsyInc.Enclave.Core.Models;

public enum ProductStatus
{
    /// <summary>
    /// The product is active and available for use.
    /// </summary>
    Active = 0,

    /// <summary>
    /// The product has been retired and is no longer available for use.
    /// </summary>
    Retired = 1,

    /// <summary>
    /// The product is upcoming and not yet available for use.
    /// </summary>
    Upcoming = 2,
}
