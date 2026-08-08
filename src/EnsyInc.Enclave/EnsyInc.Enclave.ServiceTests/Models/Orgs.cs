using System.Text.Json.Serialization;

namespace EnsyInc.Enclave.ServiceTests.Models;

public sealed record CreateOrgRequest(
    string Name);

public sealed record UpdateOrgRequest(
    string Name);

public sealed record GetOrgResponse(
    Guid Id,
    string Name,
    [property: JsonRequired] OrgStatus Status,
    Guid? PrimaryUserId,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record GetOrgsResponse(IEnumerable<GetOrgResponse> Orgs);
