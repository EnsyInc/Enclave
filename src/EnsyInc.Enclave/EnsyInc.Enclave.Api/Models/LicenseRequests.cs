using System.Text.Json.Serialization;

using EnsyInc.Enclave.Core.Models;

using JetBrains.Annotations;

namespace EnsyInc.Enclave.Api.Models;

/// <summary>
/// Request to get a list of license requests, optionally filtered by org, product, and/or status.
/// </summary>
/// <param name="OrgId">When provided, only requests belonging to this org are returned.</param>
/// <param name="ProductId">When provided, only requests for this product are returned.</param>
/// <param name="Status">When provided, only requests with this status are returned.</param>
[PublicAPI]
public sealed record GetLicenseRequestsRequest(
    Guid? OrgId,
    Guid? ProductId,
    LicenseRequestStatus? Status);

/// <summary>
/// Fields for approving a license request. <see cref="Start"/> is required for a new-license request
/// (one with no existing license reference) and is ignored for a renewal, whose license keeps its original start date.
/// </summary>
/// <param name="Start">The new license's start date, in UTC. Required for new-license requests; ignored for renewals.</param>
/// <param name="End">The license's new expiry date, in UTC.</param>
[PublicAPI]
public sealed record ApproveLicenseRequestRequest(
    DateTime? Start,
    [property: JsonRequired] DateTime End);

/// <summary>Fields for rejecting a license request.</summary>
/// <param name="Reason">An optional, customer-visible reason for the rejection.</param>
[PublicAPI]
public sealed record RejectLicenseRequestRequest(
    string? Reason);

/// <summary>A single license request.</summary>
/// <param name="Id">The request's unique identifier.</param>
/// <param name="OrgId">The org the request was submitted for.</param>
/// <param name="ProductId">The product being requested.</param>
/// <param name="UserId">The user who submitted the request.</param>
/// <param name="ExistingLicenseId">The license being renewed, if this is a renewal request. Null for a new-license request.</param>
/// <param name="RequestNotes">Free-text notes provided by the requester, if any.</param>
/// <param name="Status">The request's current status.</param>
/// <param name="RejectionReason">The reason given when the request was rejected, if applicable.</param>
/// <param name="CreatedAt">When the request was created, in UTC.</param>
/// <param name="UpdatedAt">When the request was last updated, in UTC. Null if it has never been updated.</param>
[PublicAPI]
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

/// <summary>A page of license requests.</summary>
/// <param name="LicenseRequests">The matching license requests.</param>
[PublicAPI]
public sealed record GetLicenseRequestsResponse(IEnumerable<GetLicenseRequestResponse> LicenseRequests);
