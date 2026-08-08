using System.Net;
using System.Net.Http.Json;

using EnsyInc.Enclave.ServiceTests.Fixtures;
using EnsyInc.Enclave.ServiceTests.Models;

namespace EnsyInc.Enclave.ServiceTests.LicenseRequests;

[Collection(ApiCollectionDefinition.Name)]
public sealed class GetLicenseRequestsTests(ApiFixture fixture) : LicenseRequestsApiTestBase(fixture)
{
    [Fact]
    public async Task GetLicenseRequests_FilteredByOrgId_ReturnsOnlyMatchingRequests()
    {
        var ct = TestContext.Current.CancellationToken;
        var orgA = await CreateOrg(ct);
        var orgB = await CreateOrg(ct);
        var productA = await CreateProduct(ct);
        var productB = await CreateProduct(ct);
        var userA = await CreateUser(orgA, ct);
        var userB = await CreateUser(orgB, ct);
        var inOrgA = await SeedNewLicenseRequest(orgA, productA, userA, ct);
        await SeedNewLicenseRequest(orgB, productB, userB, ct);

        var response = await Fixture.Client.GetAsync($"/license-requests?orgId={orgA}", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GetLicenseRequestsResponse>(ApiFixture.JsonOptions, ct);
        Assert.Contains(body!.LicenseRequests, r => r.Id == inOrgA);
        Assert.All(body.LicenseRequests, r => Assert.Equal(orgA, r.OrgId));
    }

    [Fact]
    public async Task GetLicenseRequests_FilteredByStatus_ReturnsOnlyMatchingRequests()
    {
        var ct = TestContext.Current.CancellationToken;
        var orgId = await CreateOrg(ct);
        var productId = await CreateProduct(ct);
        var userId = await CreateUser(orgId, ct);
        var rejected = await SeedNewLicenseRequest(orgId, productId, userId, ct);
        await Fixture.Client.PostAsJsonAsync($"/license-requests/{rejected}/reject", new RejectLicenseRequestRequest(null), ct);

        var response = await Fixture.Client.GetAsync("/license-requests?status=Rejected", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GetLicenseRequestsResponse>(ApiFixture.JsonOptions, ct);
        Assert.Contains(body!.LicenseRequests, r => r.Id == rejected);
    }
}
