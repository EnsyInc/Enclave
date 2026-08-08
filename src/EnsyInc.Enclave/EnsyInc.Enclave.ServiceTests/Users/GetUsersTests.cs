using System.Net;
using System.Net.Http.Json;

using EnsyInc.Enclave.ServiceTests.Fixtures;
using EnsyInc.Enclave.ServiceTests.Models;

namespace EnsyInc.Enclave.ServiceTests.Users;

[Collection(ApiCollectionDefinition.Name)]
public sealed class GetUsersTests(ApiFixture fixture) : UsersApiTestBase(fixture)
{
    [Fact]
    public async Task GetUsers_NonexistentOrgId_ReturnsNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var response = await Fixture.Client.GetAsync($"/orgs/{Guid.NewGuid()}/users", ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("OrgNotFound", error!.ErrorCode);
    }

    [Fact]
    public async Task GetUsers_NoFilter_IncludesInvitedUser()
    {
        var ct = TestContext.Current.CancellationToken;
        var orgId = await CreateOrg(ct);
        var invited = await InviteUser(orgId, "List Target", UserRole.Reader, ct);

        var response = await Fixture.Client.GetAsync($"/orgs/{orgId}/users", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GetUsersResponse>(ApiFixture.JsonOptions, ct);
        Assert.Contains(body!.Users, u => u.Id == invited.Id);
    }
}
