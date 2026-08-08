using System.Net;
using System.Net.Http.Json;

using EnsyInc.Enclave.ServiceTests.Fixtures;
using EnsyInc.Enclave.ServiceTests.Models;

namespace EnsyInc.Enclave.ServiceTests.Users;

[Collection(ApiCollectionDefinition.Name)]
public sealed class ReactivateUserTests(ApiFixture fixture) : UsersApiTestBase(fixture)
{
    [Fact]
    public async Task ReactivateUser_AfterDeactivate_MarksStatusActive()
    {
        var ct = TestContext.Current.CancellationToken;
        var orgId = await CreateOrg(ct);
        var created = await InviteUser(orgId, "To Reactivate", UserRole.Reader, ct);
        await Fixture.Client.PostAsync($"/orgs/{orgId}/users/{created.Id}/deactivate", content: null, ct);

        var response = await Fixture.Client.PostAsync($"/orgs/{orgId}/users/{created.Id}/reactivate", content: null, ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var reactivated = await response.Content.ReadFromJsonAsync<GetUserResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal(UserStatus.Active, reactivated!.Status);
    }

    [Fact]
    public async Task ReactivateUser_NonexistentId_ReturnsNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var orgId = await CreateOrg(ct);

        var response = await Fixture.Client.PostAsync($"/orgs/{orgId}/users/{Guid.NewGuid()}/reactivate", content: null, ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("UserNotFound", error!.ErrorCode);
    }
}
