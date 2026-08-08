using System.Text.Json.Serialization;

namespace EnsyInc.Enclave.ServiceTests.Models;

public sealed record InviteUserRequest(
    string Name,
    string Email,
    [property: JsonRequired] UserRole Role);

public sealed record InviteUsersRequest(
    IEnumerable<InviteUserRequest> Users);

public sealed record UpdateUserRequest(
    string Name,
    [property: JsonRequired] UserRole Role);

public sealed record GetUserResponse(
    Guid Id,
    string Name,
    string Email,
    Guid OrgId,
    [property: JsonRequired] UserStatus Status,
    [property: JsonRequired] UserRole Role,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record GetUsersResponse(IEnumerable<GetUserResponse> Users);
