using System.Net;
using System.Net.Http.Json;

using EnsyInc.Enclave.ServiceTests.Fixtures;
using EnsyInc.Enclave.ServiceTests.Models;

namespace EnsyInc.Enclave.ServiceTests.LicenseRequests;

[Collection(ApiCollectionDefinition.Name)]
public sealed class GetLicenseRequestTests(ApiFixture fixture) : LicenseRequestsApiTestBase(fixture)
{
    [Fact]
    public async Task GetLicenseRequest_NonexistentId_ReturnsNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var response = await Fixture.Client.GetAsync($"/license-requests/{Guid.NewGuid()}", ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("LicenseRequestNotFound", error!.ErrorCode);
    }

    [Fact]
    public async Task GetLicenseRequest_SeededDirectlyViaSql_IsVisibleThroughApi()
    {
        var ct = TestContext.Current.CancellationToken;
        var orgId = await CreateOrg(ct);
        var productId = await CreateProduct(ct);
        var userId = await CreateUser(orgId, ct);
        var requestId = await SeedNewLicenseRequest(orgId, productId, userId, ct);

        var response = await Fixture.Client.GetAsync($"/license-requests/{requestId}", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var fetched = await response.Content.ReadFromJsonAsync<GetLicenseRequestResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal(orgId, fetched!.OrgId);
        Assert.Equal(userId, fetched.UserId);
        Assert.Null(fetched.ExistingLicenseId);
    }
}
