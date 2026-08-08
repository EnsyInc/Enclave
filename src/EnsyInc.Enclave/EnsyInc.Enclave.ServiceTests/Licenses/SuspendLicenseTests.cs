using System.Net;
using System.Net.Http.Json;

using EnsyInc.Enclave.ServiceTests.Fixtures;
using EnsyInc.Enclave.ServiceTests.Models;

namespace EnsyInc.Enclave.ServiceTests.Licenses;

[Collection(ApiCollectionDefinition.Name)]
public sealed class SuspendLicenseTests(ApiFixture fixture) : LicensesApiTestBase(fixture)
{
    [Fact]
    public async Task SuspendLicense_MarksStatusSuspended()
    {
        var ct = TestContext.Current.CancellationToken;
        var orgId = await CreateOrg(ct);
        var productId = await CreateProduct(ct);
        var created = await GrantLicense(orgId, productId, DateTime.UtcNow, DateTime.UtcNow.AddYears(1), ct);

        var response = await Fixture.Client.PostAsync($"/licenses/{created.Id}/suspend", content: null, ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var suspended = await response.Content.ReadFromJsonAsync<GetLicenseResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal(LicenseStatus.Suspended, suspended!.Status);
    }

    [Fact]
    public async Task SuspendLicense_NonexistentId_ReturnsNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var response = await Fixture.Client.PostAsync($"/licenses/{Guid.NewGuid()}/suspend", content: null, ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("LicenseNotFound", error!.ErrorCode);
    }
}
