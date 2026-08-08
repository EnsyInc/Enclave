using System.Text.Json.Serialization;

namespace EnsyInc.Enclave.ServiceTests.Models;

public sealed record GrantLicenseRequest(
    Guid OrgId,
    Guid ProductId,
    DateTime Start,
    DateTime End);

public sealed record UpdateLicenseDatesRequest(
    DateTime Start,
    DateTime End);

public sealed record GetLicenseResponse(
    Guid Id,
    Guid OrgId,
    Guid ProductId,
    [property: JsonRequired] LicenseStatus Status,
    DateTime Start,
    DateTime End,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record GetLicensesResponse(IEnumerable<GetLicenseResponse> Licenses);
