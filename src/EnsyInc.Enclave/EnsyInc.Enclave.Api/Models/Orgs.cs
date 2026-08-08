using System.Text.Json.Serialization;

using EnsyInc.Enclave.Core.Models;

using JetBrains.Annotations;

namespace EnsyInc.Enclave.Api.Models;

/// <summary>
/// Request to get a list of orgs, optionally filtered by name.
/// </summary>
/// <param name="Name">When provided, only orgs whose name contains this value are returned. Case-insensitive.</param>
[PublicAPI]
public sealed record GetOrgsRequest(
    string? Name);

/// <summary>Fields for creating a new org.</summary>
/// <param name="Name">The org's display name.</param>
[PublicAPI]
public sealed record CreateOrgRequest(
    string Name);

/// <summary>Fields for updating an existing org.</summary>
/// <param name="Name">The org's display name.</param>
[PublicAPI]
public sealed record UpdateOrgRequest(
    string Name);

/// <summary>A single org.</summary>
/// <param name="Id">The org's unique identifier.</param>
/// <param name="Name">The org's display name.</param>
/// <param name="Status">Whether the org is currently active or deactivated.</param>
/// <param name="PrimaryUserId">The id of the org's primary contact user, if one has been set.</param>
/// <param name="CreatedAt">When the org was created, in UTC.</param>
/// <param name="UpdatedAt">When the org was last updated, in UTC. Null if it has never been updated.</param>
[PublicAPI]
public sealed record GetOrgResponse(
    Guid Id,
    string Name,
    [property: JsonRequired] OrgStatus Status,
    Guid? PrimaryUserId,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

/// <summary>A page of orgs.</summary>
/// <param name="Orgs">The matching orgs.</param>
[PublicAPI]
public sealed record GetOrgsResponse(IEnumerable<GetOrgResponse> Orgs);
