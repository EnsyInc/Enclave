using System.Net;
using System.Net.Http.Json;

using EnsyInc.Enclave.ServiceTests.Fixtures;
using EnsyInc.Enclave.ServiceTests.Models;

namespace EnsyInc.Enclave.ServiceTests.Users;

[Collection(ApiCollectionDefinition.Name)]
public sealed class UpdateUserTests(ApiFixture fixture) : UsersApiTestBase(fixture)
{
    [Fact]
    public async Task UpdateUser_ValidBody_ReflectsInSubsequentGet()
    {
        var ct = TestContext.Current.CancellationToken;
        var orgId = await CreateOrg(ct);
        var created = await InviteUser(orgId, "Before Update", UserRole.Reader, ct);

        var putResponse = await Fixture.Client.PutAsJsonAsync($"/orgs/{orgId}/users/{created.Id}", new UpdateUserRequest("After Update", UserRole.Admin), ct);
        Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);

        var getResponse = await Fixture.Client.GetAsync($"/orgs/{orgId}/users/{created.Id}", ct);
        var fetched = await getResponse.Content.ReadFromJsonAsync<GetUserResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("After Update", fetched!.Name);
        Assert.Equal(UserRole.Admin, fetched.Role);
    }

    [Fact]
    public async Task UpdateUser_NonexistentId_ReturnsNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var orgId = await CreateOrg(ct);

        var response = await Fixture.Client.PutAsJsonAsync($"/orgs/{orgId}/users/{Guid.NewGuid()}", new UpdateUserRequest("Ghost", UserRole.Reader), ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("UserNotFound", error!.ErrorCode);
    }
}
