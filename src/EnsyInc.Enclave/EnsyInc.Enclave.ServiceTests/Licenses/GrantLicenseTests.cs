using System.Net;
using System.Net.Http.Json;

using EnsyInc.Enclave.ServiceTests.Fixtures;
using EnsyInc.Enclave.ServiceTests.Models;

namespace EnsyInc.Enclave.ServiceTests.Licenses;

[Collection(ApiCollectionDefinition.Name)]
public sealed class GrantLicenseTests(ApiFixture fixture) : LicensesApiTestBase(fixture)
{
    [Fact]
    public async Task GrantLicense_ValidBody_ReturnsCreatedAndLocationRoundTrips()
    {
        var ct = TestContext.Current.CancellationToken;
        var orgId = await CreateOrg(ct);
        var productId = await CreateProduct(ct);
        var start = DateTime.UtcNow.AddDays(-1);
        var end = DateTime.UtcNow.AddYears(1);

        var response = await Fixture.Client.PostAsJsonAsync("/licenses", new GrantLicenseRequest(orgId, productId, start, end), ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<GetLicenseResponse>(ApiFixture.JsonOptions, ct);
        Assert.NotNull(created);
        Assert.Equal(orgId, created!.OrgId);
        Assert.Equal(productId, created.ProductId);
        Assert.Equal(LicenseStatus.Active, created.Status);
        CreatedLicenseIds.Add(created.Id);

        Assert.NotNull(response.Headers.Location);
        var getResponse = await Fixture.Client.GetAsync(response.Headers.Location, ct);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var fetched = await getResponse.Content.ReadFromJsonAsync<GetLicenseResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal(created.Id, fetched!.Id);
    }

    [Fact]
    public async Task GrantLicense_FutureStart_ReturnsScheduledStatus()
    {
        var ct = TestContext.Current.CancellationToken;
        var orgId = await CreateOrg(ct);
        var productId = await CreateProduct(ct);

        var created = await GrantLicense(orgId, productId, DateTime.UtcNow.AddDays(30), DateTime.UtcNow.AddYears(1), ct);

        Assert.Equal(LicenseStatus.Scheduled, created.Status);
    }

    [Fact]
    public async Task GrantLicense_EndBeforeStart_ReturnsBadRequestWithValidationError()
    {
        var ct = TestContext.Current.CancellationToken;
        var orgId = await CreateOrg(ct);
        var productId = await CreateProduct(ct);

        var response = await Fixture.Client.PostAsJsonAsync("/licenses", new GrantLicenseRequest(orgId, productId, DateTime.UtcNow, DateTime.UtcNow.AddDays(-1)), ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("ValidationError", error!.ErrorCode);
    }

    [Fact]
    public async Task GrantLicense_NonexistentOrgId_ReturnsNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var productId = await CreateProduct(ct);

        var response = await Fixture.Client.PostAsJsonAsync("/licenses", new GrantLicenseRequest(Guid.NewGuid(), productId, DateTime.UtcNow, DateTime.UtcNow.AddYears(1)), ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("OrgNotFound", error!.ErrorCode);
    }

    [Fact]
    public async Task GrantLicense_NonexistentProductId_ReturnsNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var orgId = await CreateOrg(ct);

        var response = await Fixture.Client.PostAsJsonAsync("/licenses", new GrantLicenseRequest(orgId, Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow.AddYears(1)), ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("ProductNotFound", error!.ErrorCode);
    }

    [Fact]
    public async Task GrantLicense_DuplicateOrgProduct_ReturnsConflictWithExistingLicenseId()
    {
        var ct = TestContext.Current.CancellationToken;
        var orgId = await CreateOrg(ct);
        var productId = await CreateProduct(ct);
        var first = await GrantLicense(orgId, productId, DateTime.UtcNow, DateTime.UtcNow.AddYears(1), ct);

        var response = await Fixture.Client.PostAsJsonAsync("/licenses", new GrantLicenseRequest(orgId, productId, DateTime.UtcNow, DateTime.UtcNow.AddYears(1)), ct);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("LicenseAlreadyExists", error!.ErrorCode);
        Assert.Equal(first.Id.ToString(), error.Parameters["ExistingLicenseId"]);
    }
}
