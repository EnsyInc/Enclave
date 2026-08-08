using System.Net;
using System.Net.Http.Json;

using EnsyInc.Enclave.ServiceTests.Fixtures;
using EnsyInc.Enclave.ServiceTests.Models;

namespace EnsyInc.Enclave.ServiceTests.Orgs;

[Collection(ApiCollectionDefinition.Name)]
public sealed class DeactivateOrgTests(ApiFixture fixture) : OrgsApiTestBase(fixture)
{
    [Fact]
    public async Task DeactivateOrg_MarksStatusDeactivated()
    {
        var ct = TestContext.Current.CancellationToken;
        var created = await CreateOrg($"To-Deactivate-{Guid.NewGuid()}", ct);

        var response = await Fixture.Client.PostAsync($"/orgs/{created.Id}/deactivate", content: null, ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var deactivated = await response.Content.ReadFromJsonAsync<GetOrgResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal(OrgStatus.Deactivated, deactivated!.Status);
    }

    [Fact]
    public async Task DeactivateOrg_NonexistentId_ReturnsNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var response = await Fixture.Client.PostAsync($"/orgs/{Guid.NewGuid()}/deactivate", content: null, ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("OrgNotFound", error!.ErrorCode);
    }
}
