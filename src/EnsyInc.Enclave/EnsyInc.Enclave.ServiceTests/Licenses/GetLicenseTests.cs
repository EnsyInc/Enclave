using System.Net;
using System.Net.Http.Json;

using EnsyInc.Enclave.ServiceTests.Fixtures;
using EnsyInc.Enclave.ServiceTests.Helpers;
using EnsyInc.Enclave.ServiceTests.Models;

namespace EnsyInc.Enclave.ServiceTests.Licenses;

[Collection(ApiCollectionDefinition.Name)]
public sealed class GetLicenseTests(ApiFixture fixture) : LicensesApiTestBase(fixture)
{
    [Fact]
    public async Task GetLicense_NonexistentId_ReturnsNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var response = await Fixture.Client.GetAsync($"/licenses/{Guid.NewGuid()}", ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("LicenseNotFound", error!.ErrorCode);
    }

    [Fact]
    public async Task GetLicense_SeededDirectlyViaSql_IsVisibleThroughApi()
    {
        var ct = TestContext.Current.CancellationToken;
        var orgId = await CreateOrg(ct);
        var productId = await CreateProduct(ct);
        var id = await LicenseSeeder.InsertLicense(Fixture.DbConnectionString, orgId, productId, DateTime.UtcNow.AddDays(-100), DateTime.UtcNow.AddDays(-1), LicenseStatus.Expired, ct);
        CreatedLicenseIds.Add(id);

        var response = await Fixture.Client.GetAsync($"/licenses/{id}", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var fetched = await response.Content.ReadFromJsonAsync<GetLicenseResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal(LicenseStatus.Expired, fetched!.Status);
    }
}
