using System.Net;
using System.Net.Http.Json;

using EnsyInc.Enclave.ServiceTests.Fixtures;
using EnsyInc.Enclave.ServiceTests.Models;

namespace EnsyInc.Enclave.ServiceTests.Users;

[Collection(ApiCollectionDefinition.Name)]
public sealed class InviteUsersTests(ApiFixture fixture) : UsersApiTestBase(fixture)
{
    [Fact]
    public async Task InviteUsers_ValidBatch_ReturnsAllCreatedUsers()
    {
        var ct = TestContext.Current.CancellationToken;
        var orgId = await CreateOrg(ct);
        var request = new InviteUsersRequest(
        [
            new InviteUserRequest("Batch One", $"{Guid.NewGuid()}@example.com", UserRole.Reader),
            new InviteUserRequest("Batch Two", $"{Guid.NewGuid()}@example.com", UserRole.Admin),
        ]);

        var response = await Fixture.Client.PostAsJsonAsync($"/orgs/{orgId}/users/batch", request, ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GetUsersResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal(2, body!.Users.Count());
        Assert.Contains(body.Users, u => u.Name == "Batch One");
        Assert.Contains(body.Users, u => u.Name == "Batch Two");
    }

    [Fact]
    public async Task InviteUsers_NonexistentOrgId_ReturnsNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var request = new InviteUsersRequest([new InviteUserRequest("Ghost", $"{Guid.NewGuid()}@example.com", UserRole.Reader)]);

        var response = await Fixture.Client.PostAsJsonAsync($"/orgs/{Guid.NewGuid()}/users/batch", request, ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("OrgNotFound", error!.ErrorCode);
    }
}
