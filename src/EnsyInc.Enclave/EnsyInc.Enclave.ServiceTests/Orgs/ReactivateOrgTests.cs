using System.Net;
using System.Net.Http.Json;

using EnsyInc.Enclave.ServiceTests.Fixtures;
using EnsyInc.Enclave.ServiceTests.Models;

namespace EnsyInc.Enclave.ServiceTests.Orgs;

[Collection(ApiCollectionDefinition.Name)]
public sealed class ReactivateOrgTests(ApiFixture fixture) : OrgsApiTestBase(fixture)
{
    [Fact]
    public async Task ReactivateOrg_AfterDeactivate_MarksStatusActive()
    {
        var ct = TestContext.Current.CancellationToken;
        var created = await CreateOrg($"To-Reactivate-{Guid.NewGuid()}", ct);
        await Fixture.Client.PostAsync($"/orgs/{created.Id}/deactivate", content: null, ct);

        var response = await Fixture.Client.PostAsync($"/orgs/{created.Id}/reactivate", content: null, ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var reactivated = await response.Content.ReadFromJsonAsync<GetOrgResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal(OrgStatus.Active, reactivated!.Status);
    }

    [Fact]
    public async Task ReactivateOrg_NonexistentId_ReturnsNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var response = await Fixture.Client.PostAsync($"/orgs/{Guid.NewGuid()}/reactivate", content: null, ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("OrgNotFound", error!.ErrorCode);
    }
}
