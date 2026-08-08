using System.Net;
using System.Net.Http.Json;

using EnsyInc.Enclave.ServiceTests.Fixtures;
using EnsyInc.Enclave.ServiceTests.Models;

namespace EnsyInc.Enclave.ServiceTests.Licenses;

[Collection(ApiCollectionDefinition.Name)]
public sealed class GetLicensesTests(ApiFixture fixture) : LicensesApiTestBase(fixture)
{
    [Fact]
    public async Task GetLicenses_FilteredByOrgId_ReturnsOnlyMatchingLicenses()
    {
        var ct = TestContext.Current.CancellationToken;
        var orgA = await CreateOrg(ct);
        var orgB = await CreateOrg(ct);
        var productA = await CreateProduct(ct);
        var productB = await CreateProduct(ct);
        var inOrgA = await GrantLicense(orgA, productA, DateTime.UtcNow, DateTime.UtcNow.AddYears(1), ct);
        await GrantLicense(orgB, productB, DateTime.UtcNow, DateTime.UtcNow.AddYears(1), ct);

        var response = await Fixture.Client.GetAsync($"/licenses?orgId={orgA}", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GetLicensesResponse>(ApiFixture.JsonOptions, ct);
        Assert.Contains(body!.Licenses, l => l.Id == inOrgA.Id);
        Assert.All(body.Licenses, l => Assert.Equal(orgA, l.OrgId));
    }

    [Fact]
    public async Task GetLicenses_FilteredByStatus_ReturnsOnlyMatchingLicenses()
    {
        var ct = TestContext.Current.CancellationToken;
        var orgId = await CreateOrg(ct);
        var productId = await CreateProduct(ct);
        var license = await GrantLicense(orgId, productId, DateTime.UtcNow, DateTime.UtcNow.AddYears(1), ct);
        await Fixture.Client.PostAsync($"/licenses/{license.Id}/suspend", content: null, ct);

        var response = await Fixture.Client.GetAsync("/licenses?status=Suspended", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GetLicensesResponse>(ApiFixture.JsonOptions, ct);
        Assert.Contains(body!.Licenses, l => l.Id == license.Id);
    }
}
