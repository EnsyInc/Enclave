using System.Text.Json.Serialization;

namespace EnsyInc.Enclave.ServiceTests.Models;

public sealed record ApproveLicenseRequestRequest(
    DateTime? Start,
    DateTime End);

public sealed record RejectLicenseRequestRequest(
    string? Reason);

public sealed record GetLicenseRequestResponse(
    Guid Id,
    Guid OrgId,
    Guid ProductId,
    Guid UserId,
    Guid? ExistingLicenseId,
    string? RequestNotes,
    [property: JsonRequired] LicenseRequestStatus Status,
    string? RejectionReason,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record GetLicenseRequestsResponse(IEnumerable<GetLicenseRequestResponse> LicenseRequests);
