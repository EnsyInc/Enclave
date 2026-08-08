using System.Net;
using System.Net.Http.Json;

using EnsyInc.Enclave.ServiceTests.Fixtures;
using EnsyInc.Enclave.ServiceTests.Models;

namespace EnsyInc.Enclave.ServiceTests.LicenseRequests;

[Collection(ApiCollectionDefinition.Name)]
public sealed class ApproveLicenseRequestTests(ApiFixture fixture) : LicenseRequestsApiTestBase(fixture)
{
    [Fact]
    public async Task ApproveLicenseRequest_NewRequest_GrantsLicenseWithGivenDates()
    {
        var ct = TestContext.Current.CancellationToken;
        var orgId = await CreateOrg(ct);
        var productId = await CreateProduct(ct);
        var userId = await CreateUser(orgId, ct);
        var requestId = await SeedNewLicenseRequest(orgId, productId, userId, ct);
        var start = DateTime.UtcNow;
        var end = DateTime.UtcNow.AddYears(1);

        var response = await Fixture.Client.PostAsJsonAsync($"/license-requests/{requestId}/approve", new ApproveLicenseRequestRequest(start, end), ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var approved = await response.Content.ReadFromJsonAsync<GetLicenseRequestResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal(LicenseRequestStatus.Approved, approved!.Status);

        var licensesResponse = await Fixture.Client.GetAsync($"/licenses?orgId={orgId}&productId={productId}", ct);
        var licensesBody = await licensesResponse.Content.ReadFromJsonAsync<GetLicensesResponse>(ApiFixture.JsonOptions, ct);
        var grantedLicense = Assert.Single(licensesBody!.Licenses);
        Assert.Equal(LicenseStatus.Active, grantedLicense.Status);
        CreatedLicenseIds.Add(grantedLicense.Id);
    }

    [Fact]
    public async Task ApproveLicenseRequest_NewRequest_MissingStart_ReturnsBadRequest()
    {
        var ct = TestContext.Current.CancellationToken;
        var orgId = await CreateOrg(ct);
        var productId = await CreateProduct(ct);
        var userId = await CreateUser(orgId, ct);
        var requestId = await SeedNewLicenseRequest(orgId, productId, userId, ct);

        var response = await Fixture.Client.PostAsJsonAsync($"/license-requests/{requestId}/approve", new ApproveLicenseRequestRequest(null, DateTime.UtcNow.AddYears(1)), ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("LicenseRequestStartDateRequired", error!.ErrorCode);
    }

    [Fact]
    public async Task ApproveLicenseRequest_NewRequest_DuplicateOrgProduct_ReturnsConflictWithExistingLicenseId()
    {
        var ct = TestContext.Current.CancellationToken;
        var orgId = await CreateOrg(ct);
        var productId = await CreateProduct(ct);
        var userId = await CreateUser(orgId, ct);
        var existingLicense = await GrantLicense(orgId, productId, DateTime.UtcNow, DateTime.UtcNow.AddYears(1), ct);
        var requestId = await SeedNewLicenseRequest(orgId, productId, userId, ct);

        var response = await Fixture.Client.PostAsJsonAsync($"/license-requests/{requestId}/approve", new ApproveLicenseRequestRequest(DateTime.UtcNow, DateTime.UtcNow.AddYears(1)), ct);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("LicenseAlreadyExists", error!.ErrorCode);
        Assert.Equal(existingLicense.Id.ToString(), error.Parameters["ExistingLicenseId"]);
    }

    [Fact]
    public async Task ApproveLicenseRequest_Renewal_ExtendsExistingLicenseEndAndKeepsStart()
    {
        var ct = TestContext.Current.CancellationToken;
        var orgId = await CreateOrg(ct);
        var productId = await CreateProduct(ct);
        var userId = await CreateUser(orgId, ct);
        var (requestId, licenseId) = await SeedRenewalLicenseRequest(orgId, productId, userId, ct);
        var beforeResponse = await Fixture.Client.GetAsync($"/licenses/{licenseId}", ct);
        var before = (await beforeResponse.Content.ReadFromJsonAsync<GetLicenseResponse>(ApiFixture.JsonOptions, ct))!;
        var newEnd = DateTime.UtcNow.AddYears(1);

        var response = await Fixture.Client.PostAsJsonAsync($"/license-requests/{requestId}/approve", new ApproveLicenseRequestRequest(null, newEnd), ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var approved = await response.Content.ReadFromJsonAsync<GetLicenseRequestResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal(LicenseRequestStatus.Approved, approved!.Status);

        var afterResponse = await Fixture.Client.GetAsync($"/licenses/{licenseId}", ct);
        var after = (await afterResponse.Content.ReadFromJsonAsync<GetLicenseResponse>(ApiFixture.JsonOptions, ct))!;
        Assert.Equal(LicenseStatus.Active, after.Status);
        Assert.True(after.End > before.End);
        Assert.Equal(before.Start, after.Start);
    }

    [Fact]
    public async Task ApproveLicenseRequest_Renewal_EndBeforeExistingStart_ReturnsBadRequest()
    {
        var ct = TestContext.Current.CancellationToken;
        var orgId = await CreateOrg(ct);
        var productId = await CreateProduct(ct);
        var userId = await CreateUser(orgId, ct);
        var (requestId, licenseId) = await SeedRenewalLicenseRequest(orgId, productId, userId, ct);
        var licenseResponse = await Fixture.Client.GetAsync($"/licenses/{licenseId}", ct);
        var license = (await licenseResponse.Content.ReadFromJsonAsync<GetLicenseResponse>(ApiFixture.JsonOptions, ct))!;

        var response = await Fixture.Client.PostAsJsonAsync($"/license-requests/{requestId}/approve", new ApproveLicenseRequestRequest(null, license.Start.AddDays(-1)), ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("LicenseRequestInvalidDateRange", error!.ErrorCode);
    }

    [Fact]
    public async Task ApproveLicenseRequest_AlreadyReviewed_ReturnsConflict()
    {
        var ct = TestContext.Current.CancellationToken;
        var orgId = await CreateOrg(ct);
        var productId = await CreateProduct(ct);
        var userId = await CreateUser(orgId, ct);
        var requestId = await SeedNewLicenseRequest(orgId, productId, userId, ct);
        var firstApprove = await Fixture.Client.PostAsJsonAsync($"/license-requests/{requestId}/approve", new ApproveLicenseRequestRequest(DateTime.UtcNow, DateTime.UtcNow.AddYears(1)), ct);
        firstApprove.EnsureSuccessStatusCode();

        var response = await Fixture.Client.PostAsJsonAsync($"/license-requests/{requestId}/approve", new ApproveLicenseRequestRequest(DateTime.UtcNow, DateTime.UtcNow.AddYears(1)), ct);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("LicenseRequestNotPending", error!.ErrorCode);

        var licensesResponse = await Fixture.Client.GetAsync($"/licenses?orgId={orgId}&productId={productId}", ct);
        var licensesBody = await licensesResponse.Content.ReadFromJsonAsync<GetLicensesResponse>(ApiFixture.JsonOptions, ct);
        foreach (var license in licensesBody!.Licenses)
        {
            CreatedLicenseIds.Add(license.Id);
        }
    }

    [Fact]
    public async Task ApproveLicenseRequest_NonexistentId_ReturnsNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var response = await Fixture.Client.PostAsJsonAsync($"/license-requests/{Guid.NewGuid()}/approve", new ApproveLicenseRequestRequest(DateTime.UtcNow, DateTime.UtcNow.AddYears(1)), ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("LicenseRequestNotFound", error!.ErrorCode);
    }
}
