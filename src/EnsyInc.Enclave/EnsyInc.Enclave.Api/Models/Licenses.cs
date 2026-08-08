using System.Text.Json.Serialization;

using EnsyInc.Enclave.Core.Models;

using JetBrains.Annotations;

namespace EnsyInc.Enclave.Api.Models;

/// <summary>
/// Request to get a list of licenses, optionally filtered by org, product, and/or status.
/// </summary>
/// <param name="OrgId">When provided, only licenses belonging to this org are returned.</param>
/// <param name="ProductId">When provided, only licenses for this product are returned.</param>
/// <param name="Status">When provided, only licenses with this status are returned.</param>
[PublicAPI]
public sealed record GetLicensesRequest(
    Guid? OrgId,
    Guid? ProductId,
    LicenseStatus? Status);

/// <summary>Fields for granting a new license.</summary>
/// <param name="OrgId">The org the license is granted to.</param>
/// <param name="ProductId">The product the license covers.</param>
/// <param name="Start">When the license becomes valid, in UTC.</param>
/// <param name="End">When the license expires, in UTC.</param>
[PublicAPI]
public sealed record GrantLicenseRequest(
    [property: JsonRequired] Guid OrgId,
    [property: JsonRequired] Guid ProductId,
    [property: JsonRequired] DateTime Start,
    [property: JsonRequired] DateTime End);

/// <summary>Fields for updating an existing license's date range.</summary>
/// <param name="Start">When the license becomes valid, in UTC.</param>
/// <param name="End">When the license expires, in UTC.</param>
[PublicAPI]
public sealed record UpdateLicenseDatesRequest(
    [property: JsonRequired] DateTime Start,
    [property: JsonRequired] DateTime End);

/// <summary>A single license.</summary>
/// <param name="Id">The license's unique identifier.</param>
/// <param name="OrgId">The org the license is granted to.</param>
/// <param name="ProductId">The product the license covers.</param>
/// <param name="Status">The license's current status.</param>
/// <param name="Start">When the license becomes valid, in UTC.</param>
/// <param name="End">When the license expires, in UTC.</param>
/// <param name="CreatedAt">When the license was created, in UTC.</param>
/// <param name="UpdatedAt">When the license was last updated, in UTC. Null if it has never been updated.</param>
[PublicAPI]
public sealed record GetLicenseResponse(
    Guid Id,
    Guid OrgId,
    Guid ProductId,
    [property: JsonRequired] LicenseStatus Status,
    DateTime Start,
    DateTime End,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

/// <summary>A page of licenses.</summary>
/// <param name="Licenses">The matching licenses.</param>
[PublicAPI]
public sealed record GetLicensesResponse(IEnumerable<GetLicenseResponse> Licenses);
