namespace EnsyInc.Enclave.ServiceTests.Models;

// Deliberately independent copies of the Api/Core enums (not a shared reference) so a breaking
// wire-contract change (renamed/removed member) fails deserialization here instead of silently
// staying in sync because it's literally the same type. See Models/ErrorResponse.cs for the same
// reasoning applied to the response/request records.

public enum ProductStatus
{
    Active = 0,
    Retired = 1,
    Upcoming = 2,
}

public enum OrgStatus
{
    Active = 0,
    Deactivated = 1,
}

public enum UserStatus
{
    InviteSent = 0,
    Active = 1,
    Deactivated = 2,
}

public enum UserRole
{
    Reader = 0,
    Admin = 1,
}
