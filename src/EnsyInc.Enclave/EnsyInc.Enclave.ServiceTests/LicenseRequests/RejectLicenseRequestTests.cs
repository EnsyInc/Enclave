using System.Net;
using System.Net.Http.Json;

using EnsyInc.Enclave.ServiceTests.Fixtures;
using EnsyInc.Enclave.ServiceTests.Models;

namespace EnsyInc.Enclave.ServiceTests.LicenseRequests;

[Collection(ApiCollectionDefinition.Name)]
public sealed class RejectLicenseRequestTests(ApiFixture fixture) : LicenseRequestsApiTestBase(fixture)
{
    [Fact]
    public async Task RejectLicenseRequest_WithReason_MarksRejectedWithReason()
    {
        var ct = TestContext.Current.CancellationToken;
        var orgId = await CreateOrg(ct);
        var productId = await CreateProduct(ct);
        var userId = await CreateUser(orgId, ct);
        var requestId = await SeedNewLicenseRequest(orgId, productId, userId, ct);

        var response = await Fixture.Client.PostAsJsonAsync($"/license-requests/{requestId}/reject", new RejectLicenseRequestRequest("Not eligible"), ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var rejected = await response.Content.ReadFromJsonAsync<GetLicenseRequestResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal(LicenseRequestStatus.Rejected, rejected!.Status);
        Assert.Equal("Not eligible", rejected.RejectionReason);
    }

    [Fact]
    public async Task RejectLicenseRequest_WithoutReason_MarksRejectedWithNullReason()
    {
        var ct = TestContext.Current.CancellationToken;
        var orgId = await CreateOrg(ct);
        var productId = await CreateProduct(ct);
        var userId = await CreateUser(orgId, ct);
        var requestId = await SeedNewLicenseRequest(orgId, productId, userId, ct);

        var response = await Fixture.Client.PostAsJsonAsync($"/license-requests/{requestId}/reject", new RejectLicenseRequestRequest(null), ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var rejected = await response.Content.ReadFromJsonAsync<GetLicenseRequestResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal(LicenseRequestStatus.Rejected, rejected!.Status);
        Assert.Null(rejected.RejectionReason);
    }

    [Fact]
    public async Task RejectLicenseRequest_AlreadyReviewed_ReturnsConflict()
    {
        var ct = TestContext.Current.CancellationToken;
        var orgId = await CreateOrg(ct);
        var productId = await CreateProduct(ct);
        var userId = await CreateUser(orgId, ct);
        var requestId = await SeedNewLicenseRequest(orgId, productId, userId, ct);
        await Fixture.Client.PostAsJsonAsync($"/license-requests/{requestId}/reject", new RejectLicenseRequestRequest(null), ct);

        var response = await Fixture.Client.PostAsJsonAsync($"/license-requests/{requestId}/reject", new RejectLicenseRequestRequest(null), ct);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("LicenseRequestNotPending", error!.ErrorCode);
    }

    [Fact]
    public async Task RejectLicenseRequest_NonexistentId_ReturnsNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var response = await Fixture.Client.PostAsJsonAsync($"/license-requests/{Guid.NewGuid()}/reject", new RejectLicenseRequestRequest(null), ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("LicenseRequestNotFound", error!.ErrorCode);
    }
}
