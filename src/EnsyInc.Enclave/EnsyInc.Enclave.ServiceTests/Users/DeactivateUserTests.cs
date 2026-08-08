using System.Net;
using System.Net.Http.Json;

using EnsyInc.Enclave.ServiceTests.Fixtures;
using EnsyInc.Enclave.ServiceTests.Models;

namespace EnsyInc.Enclave.ServiceTests.Users;

[Collection(ApiCollectionDefinition.Name)]
public sealed class DeactivateUserTests(ApiFixture fixture) : UsersApiTestBase(fixture)
{
    [Fact]
    public async Task DeactivateUser_MarksStatusDeactivated()
    {
        var ct = TestContext.Current.CancellationToken;
        var orgId = await CreateOrg(ct);
        var created = await InviteUser(orgId, "To Deactivate", UserRole.Reader, ct);

        var response = await Fixture.Client.PostAsync($"/orgs/{orgId}/users/{created.Id}/deactivate", content: null, ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var deactivated = await response.Content.ReadFromJsonAsync<GetUserResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal(UserStatus.Deactivated, deactivated!.Status);
    }

    [Fact]
    public async Task DeactivateUser_NonexistentId_ReturnsNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var orgId = await CreateOrg(ct);

        var response = await Fixture.Client.PostAsync($"/orgs/{orgId}/users/{Guid.NewGuid()}/deactivate", content: null, ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("UserNotFound", error!.ErrorCode);
    }
}
