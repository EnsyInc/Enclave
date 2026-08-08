using System.Collections.ObjectModel;
using System.Net.Http.Json;

using EnsyInc.Enclave.ServiceTests.Fixtures;
using EnsyInc.Enclave.ServiceTests.Helpers;
using EnsyInc.Enclave.ServiceTests.Models;

namespace EnsyInc.Enclave.ServiceTests.LicenseRequests;

/// <summary>Shared org/product/user/license/license-request setup and cleanup for the per-endpoint LicenseRequest test classes.</summary>
public abstract class LicenseRequestsApiTestBase(ApiFixture fixture) : IAsyncDisposable
{
    private readonly List<Guid> _createdOrgIds = [];
    private readonly List<Guid> _createdProductIds = [];

    protected ApiFixture Fixture { get; } = fixture;

    protected Collection<Guid> CreatedLicenseIds { get; } = [];

    protected Collection<Guid> CreatedLicenseRequestIds { get; } = [];

    public async ValueTask DisposeAsync()
    {
        var ct = TestContext.Current.CancellationToken;
        foreach (var id in CreatedLicenseRequestIds)
        {
            await LicenseRequestSeeder.DeleteLicenseRequest(Fixture.DbConnectionString, id, ct);
        }

        foreach (var id in CreatedLicenseIds)
        {
            await Fixture.Client.DeleteAsync($"/licenses/{id}", ct);
        }

        foreach (var id in _createdProductIds)
        {
            await Fixture.Client.DeleteAsync($"/products/{id}", ct);
        }

        foreach (var id in _createdOrgIds)
        {
            await Fixture.Client.DeleteAsync($"/orgs/{id}", ct);
        }

        GC.SuppressFinalize(this);
    }

    protected async Task<Guid> CreateOrg(CancellationToken ct)
    {
        var response = await Fixture.Client.PostAsJsonAsync("/orgs", new CreateOrgRequest($"Test-Org-{Guid.NewGuid()}"), ct);
        response.EnsureSuccessStatusCode();
        var body = (await response.Content.ReadFromJsonAsync<GetOrgResponse>(ApiFixture.JsonOptions, ct))!;
        _createdOrgIds.Add(body.Id);
        return body.Id;
    }

    protected async Task<Guid> CreateProduct(CancellationToken ct)
    {
        var response = await Fixture.Client.PostAsJsonAsync("/products", new CreateProductRequest($"Test-Product-{Guid.NewGuid()}", null, ProductStatus.Active), ct);
        response.EnsureSuccessStatusCode();
        var body = (await response.Content.ReadFromJsonAsync<GetProductResponse>(ApiFixture.JsonOptions, ct))!;
        _createdProductIds.Add(body.Id);
        return body.Id;
    }

    protected async Task<Guid> CreateUser(Guid orgId, CancellationToken ct)
    {
        var response = await Fixture.Client.PostAsJsonAsync($"/orgs/{orgId}/users", new InviteUserRequest("Requester", $"{Guid.NewGuid()}@example.com", UserRole.Reader), ct);
        response.EnsureSuccessStatusCode();
        var body = (await response.Content.ReadFromJsonAsync<GetUserResponse>(ApiFixture.JsonOptions, ct))!;
        return body.Id;
    }

    protected async Task<Guid> SeedNewLicenseRequest(Guid orgId, Guid productId, Guid userId, CancellationToken ct, LicenseRequestStatus status = LicenseRequestStatus.Pending)
    {
        var id = await LicenseRequestSeeder.InsertLicenseRequest(Fixture.DbConnectionString, orgId, productId, userId, null, "Please grant", status, ct);
        CreatedLicenseRequestIds.Add(id);
        return id;
    }

    protected async Task<(Guid RequestId, Guid LicenseId)> SeedRenewalLicenseRequest(Guid orgId, Guid productId, Guid userId, CancellationToken ct)
    {
        var licenseId = await LicenseSeeder.InsertLicense(Fixture.DbConnectionString, orgId, productId, DateTime.UtcNow.AddYears(-1), DateTime.UtcNow.AddDays(-1), LicenseStatus.Expired, ct);
        CreatedLicenseIds.Add(licenseId);
        var requestId = await LicenseRequestSeeder.InsertLicenseRequest(Fixture.DbConnectionString, orgId, productId, userId, licenseId, "Renew please", LicenseRequestStatus.Pending, ct);
        CreatedLicenseRequestIds.Add(requestId);
        return (requestId, licenseId);
    }

    protected async Task<GetLicenseResponse> GrantLicense(Guid orgId, Guid productId, DateTime start, DateTime end, CancellationToken ct)
    {
        var response = await Fixture.Client.PostAsJsonAsync("/licenses", new GrantLicenseRequest(orgId, productId, start, end), ct);
        response.EnsureSuccessStatusCode();
        var body = (await response.Content.ReadFromJsonAsync<GetLicenseResponse>(ApiFixture.JsonOptions, ct))!;
        CreatedLicenseIds.Add(body.Id);
        return body;
    }
}
