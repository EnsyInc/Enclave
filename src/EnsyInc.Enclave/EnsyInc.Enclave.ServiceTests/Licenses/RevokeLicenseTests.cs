using System.Net;
using System.Net.Http.Json;

using EnsyInc.Enclave.ServiceTests.Fixtures;
using EnsyInc.Enclave.ServiceTests.Models;

namespace EnsyInc.Enclave.ServiceTests.Licenses;

[Collection(ApiCollectionDefinition.Name)]
public sealed class RevokeLicenseTests(ApiFixture fixture) : LicensesApiTestBase(fixture)
{
    [Fact]
    public async Task RevokeLicense_MarksStatusRevoked()
    {
        var ct = TestContext.Current.CancellationToken;
        var orgId = await CreateOrg(ct);
        var productId = await CreateProduct(ct);
        var created = await GrantLicense(orgId, productId, DateTime.UtcNow, DateTime.UtcNow.AddYears(1), ct);

        var response = await Fixture.Client.PostAsync($"/licenses/{created.Id}/revoke", content: null, ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var revoked = await response.Content.ReadFromJsonAsync<GetLicenseResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal(LicenseStatus.Revoked, revoked!.Status);
    }

    [Fact]
    public async Task RevokeLicense_NonexistentId_ReturnsNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var response = await Fixture.Client.PostAsync($"/licenses/{Guid.NewGuid()}/revoke", content: null, ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("LicenseNotFound", error!.ErrorCode);
    }
}
