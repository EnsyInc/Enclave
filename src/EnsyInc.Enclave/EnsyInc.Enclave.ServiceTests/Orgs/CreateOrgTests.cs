using System.Net;
using System.Net.Http.Json;

using EnsyInc.Enclave.ServiceTests.Fixtures;
using EnsyInc.Enclave.ServiceTests.Models;

namespace EnsyInc.Enclave.ServiceTests.Orgs;

[Collection(ApiCollectionDefinition.Name)]
public sealed class CreateOrgTests(ApiFixture fixture) : OrgsApiTestBase(fixture)
{
    [Fact]
    public async Task CreateOrg_ValidBody_ReturnsCreatedAndLocationRoundTrips()
    {
        var ct = TestContext.Current.CancellationToken;
        var name = $"Test-Org-{Guid.NewGuid()}";

        var response = await Fixture.Client.PostAsJsonAsync("/orgs", new CreateOrgRequest(name), ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<GetOrgResponse>(ApiFixture.JsonOptions, ct);
        Assert.NotNull(created);
        Assert.Equal(name, created!.Name);
        Assert.Equal(OrgStatus.Active, created.Status);
        CreatedOrgIds.Add(created.Id);

        Assert.NotNull(response.Headers.Location);
        var getResponse = await Fixture.Client.GetAsync(response.Headers.Location, ct);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var fetched = await getResponse.Content.ReadFromJsonAsync<GetOrgResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal(created.Id, fetched!.Id);
    }

    [Fact]
    public async Task CreateOrg_EmptyName_ReturnsBadRequestWithValidationError()
    {
        var ct = TestContext.Current.CancellationToken;
        var response = await Fixture.Client.PostAsJsonAsync("/orgs", new CreateOrgRequest(string.Empty), ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("ValidationError", error!.ErrorCode);
    }
}
