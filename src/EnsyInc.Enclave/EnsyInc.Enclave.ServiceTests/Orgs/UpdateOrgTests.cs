using System.Net;
using System.Net.Http.Json;

using EnsyInc.Enclave.ServiceTests.Fixtures;
using EnsyInc.Enclave.ServiceTests.Models;

namespace EnsyInc.Enclave.ServiceTests.Orgs;

[Collection(ApiCollectionDefinition.Name)]
public sealed class UpdateOrgTests(ApiFixture fixture) : OrgsApiTestBase(fixture)
{
    [Fact]
    public async Task UpdateOrg_ValidBody_ReflectsInSubsequentGet()
    {
        var ct = TestContext.Current.CancellationToken;
        var created = await CreateOrg($"Before-Update-{Guid.NewGuid()}", ct);
        var newName = $"After-Update-{Guid.NewGuid()}";

        var putResponse = await Fixture.Client.PutAsJsonAsync($"/orgs/{created.Id}", new UpdateOrgRequest(newName), ct);
        Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);

        var getResponse = await Fixture.Client.GetAsync($"/orgs/{created.Id}", ct);
        var fetched = await getResponse.Content.ReadFromJsonAsync<GetOrgResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal(newName, fetched!.Name);
    }

    [Fact]
    public async Task UpdateOrg_NonexistentId_ReturnsNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var response = await Fixture.Client.PutAsJsonAsync($"/orgs/{Guid.NewGuid()}", new UpdateOrgRequest("Ghost"), ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("OrgNotFound", error!.ErrorCode);
    }
}
