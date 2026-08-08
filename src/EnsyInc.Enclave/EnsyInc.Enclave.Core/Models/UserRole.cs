namespace EnsyInc.Enclave.Core.Models;

public enum UserRole
{
    /// <summary>
    /// The user has read-only access to the system and can view data but cannot make any changes.
    /// </summary>
    Reader = 0,

    /// <summary>
    /// The user can manage their organization's info, licenses, and other users within the organization.
    /// </summary>
    Admin = 1,
}
