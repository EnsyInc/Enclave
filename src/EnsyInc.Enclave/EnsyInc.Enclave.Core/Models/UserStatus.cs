namespace EnsyInc.Enclave.Core.Models;

public enum UserStatus
{
    /// <summary>
    /// The user has been invited to join the organization but has not yet accepted the invitation.
    /// </summary>
    InviteSent = 0,

    /// <summary>
    /// The user is active and has accepted the invitation to join the organization.
    /// </summary>
    Active = 1,

    /// <summary>
    /// The user has been deactivated and cannot access the system.
    /// </summary>
    Deactivated = 2,
}
