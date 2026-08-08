using System.Text.Json.Serialization;

using EnsyInc.Enclave.Core.Models;

using JetBrains.Annotations;

namespace EnsyInc.Enclave.Api.Models;

/// <summary>Fields for inviting a new user into an org.</summary>
/// <param name="Name">The user's display name.</param>
/// <param name="Email">The user's email address.</param>
/// <param name="Role">The user's role within the org.</param>
[PublicAPI]
public sealed record InviteUserRequest(
    string Name,
    string Email,
    [property: JsonRequired] UserRole Role);

/// <summary>Fields for inviting a batch of new users into an org.</summary>
/// <param name="Users">The users to invite.</param>
[PublicAPI]
public sealed record InviteUsersRequest(
    IEnumerable<InviteUserRequest> Users);

/// <summary>Fields for updating an existing user.</summary>
/// <param name="Name">The user's display name.</param>
/// <param name="Role">The user's role within the org.</param>
[PublicAPI]
public sealed record UpdateUserRequest(
    string Name,
    [property: JsonRequired] UserRole Role);

/// <summary>A single user.</summary>
/// <param name="Id">The user's unique identifier.</param>
/// <param name="Name">The user's display name.</param>
/// <param name="Email">The user's email address.</param>
/// <param name="OrgId">The id of the org the user belongs to.</param>
/// <param name="Status">The user's lifecycle status.</param>
/// <param name="Role">The user's role within the org.</param>
/// <param name="CreatedAt">When the user was created, in UTC.</param>
/// <param name="UpdatedAt">When the user was last updated, in UTC. Null if it has never been updated.</param>
[PublicAPI]
public sealed record GetUserResponse(
    Guid Id,
    string Name,
    string Email,
    Guid OrgId,
    [property: JsonRequired] UserStatus Status,
    [property: JsonRequired] UserRole Role,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

/// <summary>A page of users.</summary>
/// <param name="Users">The matching users.</param>
[PublicAPI]
public sealed record GetUsersResponse(IEnumerable<GetUserResponse> Users);
