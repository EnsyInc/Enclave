using System.Net;
using System.Net.Http.Json;

using EnsyInc.Enclave.ServiceTests.Fixtures;
using EnsyInc.Enclave.ServiceTests.Helpers;
using EnsyInc.Enclave.ServiceTests.Models;

namespace EnsyInc.Enclave.ServiceTests.Users;

[Collection(ApiCollectionDefinition.Name)]
public sealed class UsersApiTests(ApiFixture fixture) : IAsyncDisposable
{
    private readonly List<Guid> _createdOrgIds = [];

    public async ValueTask DisposeAsync()
    {
        var ct = TestContext.Current.CancellationToken;
        foreach (var id in _createdOrgIds)
        {
            await fixture.Client.DeleteAsync($"/orgs/{id}", ct);
        }
    }

    private async Task<Guid> CreateOrg(CancellationToken ct)
    {
        var response = await fixture.Client.PostAsJsonAsync("/orgs", new CreateOrgRequest($"Test-Org-{Guid.NewGuid()}"), ct);
        response.EnsureSuccessStatusCode();
        var body = (await response.Content.ReadFromJsonAsync<GetOrgResponse>(ApiFixture.JsonOptions, ct))!;
        _createdOrgIds.Add(body.Id);
        return body.Id;
    }

    private async Task<GetUserResponse> InviteUser(Guid orgId, string name, UserRole role, CancellationToken ct)
    {
        var response = await fixture.Client.PostAsJsonAsync($"/orgs/{orgId}/users", new InviteUserRequest(name, $"{Guid.NewGuid()}@example.com", role), ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<GetUserResponse>(ApiFixture.JsonOptions, ct))!;
    }

    [Fact]
    public async Task InviteUser_ValidBody_ReturnsCreatedWithInviteSentStatusAndLocationRoundTrips()
    {
        var ct = TestContext.Current.CancellationToken;
        var orgId = await CreateOrg(ct);
        var email = $"{Guid.NewGuid()}@example.com";

        var response = await fixture.Client.PostAsJsonAsync($"/orgs/{orgId}/users", new InviteUserRequest("Jane Doe", email, UserRole.Admin), ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<GetUserResponse>(ApiFixture.JsonOptions, ct);
        Assert.NotNull(created);
        Assert.Equal(email, created!.Email);
        Assert.Equal(UserStatus.InviteSent, created.Status);
        Assert.Equal(orgId, created.OrgId);

        Assert.NotNull(response.Headers.Location);
        var getResponse = await fixture.Client.GetAsync(response.Headers.Location, ct);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var fetched = await getResponse.Content.ReadFromJsonAsync<GetUserResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal(created.Id, fetched!.Id);
    }

    [Fact]
    public async Task InviteUser_InvalidEmail_ReturnsBadRequestWithValidationError()
    {
        var ct = TestContext.Current.CancellationToken;
        var orgId = await CreateOrg(ct);

        var response = await fixture.Client.PostAsJsonAsync($"/orgs/{orgId}/users", new InviteUserRequest("Jane Doe", "not-an-email", UserRole.Reader), ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("ValidationError", error!.ErrorCode);
    }

    [Fact]
    public async Task InviteUser_NonexistentOrgId_ReturnsNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var response = await fixture.Client.PostAsJsonAsync($"/orgs/{Guid.NewGuid()}/users", new InviteUserRequest("Jane Doe", $"{Guid.NewGuid()}@example.com", UserRole.Reader), ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("OrgNotFound", error!.ErrorCode);
    }

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

        var response = await fixture.Client.PostAsJsonAsync($"/orgs/{orgId}/users/batch", request, ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GetUsersResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal(2, body!.Users.Count());
        Assert.Contains(body.Users, u => u.Name == "Batch One");
        Assert.Contains(body.Users, u => u.Name == "Batch Two");
    }

    [Fact]
    public async Task GetUsers_NonexistentOrgId_ReturnsNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var response = await fixture.Client.GetAsync($"/orgs/{Guid.NewGuid()}/users", ct);

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

        var response = await fixture.Client.GetAsync($"/orgs/{orgId}/users", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GetUsersResponse>(ApiFixture.JsonOptions, ct);
        Assert.Contains(body!.Users, u => u.Id == invited.Id);
    }

    [Fact]
    public async Task GetUser_UserFromDifferentOrg_ReturnsNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var orgA = await CreateOrg(ct);
        var orgB = await CreateOrg(ct);
        var userInOrgA = await InviteUser(orgA, "Org A User", UserRole.Reader, ct);

        var response = await fixture.Client.GetAsync($"/orgs/{orgB}/users/{userInOrgA.Id}", ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("UserNotFound", error!.ErrorCode);
    }

    [Fact]
    public async Task UpdateUser_ValidBody_ReflectsInSubsequentGet()
    {
        var ct = TestContext.Current.CancellationToken;
        var orgId = await CreateOrg(ct);
        var created = await InviteUser(orgId, "Before Update", UserRole.Reader, ct);

        var putResponse = await fixture.Client.PutAsJsonAsync($"/orgs/{orgId}/users/{created.Id}", new UpdateUserRequest("After Update", UserRole.Admin), ct);
        Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);

        var getResponse = await fixture.Client.GetAsync($"/orgs/{orgId}/users/{created.Id}", ct);
        var fetched = await getResponse.Content.ReadFromJsonAsync<GetUserResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("After Update", fetched!.Name);
        Assert.Equal(UserRole.Admin, fetched.Role);
    }

    [Fact]
    public async Task UpdateUser_NonexistentId_ReturnsNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var orgId = await CreateOrg(ct);

        var response = await fixture.Client.PutAsJsonAsync($"/orgs/{orgId}/users/{Guid.NewGuid()}", new UpdateUserRequest("Ghost", UserRole.Reader), ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("UserNotFound", error!.ErrorCode);
    }

    [Fact]
    public async Task DeactivateUser_MarksStatusDeactivated()
    {
        var ct = TestContext.Current.CancellationToken;
        var orgId = await CreateOrg(ct);
        var created = await InviteUser(orgId, "To Deactivate", UserRole.Reader, ct);

        var response = await fixture.Client.PostAsync($"/orgs/{orgId}/users/{created.Id}/deactivate", content: null, ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var deactivated = await response.Content.ReadFromJsonAsync<GetUserResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal(UserStatus.Deactivated, deactivated!.Status);
    }

    [Fact]
    public async Task ReactivateUser_AfterDeactivate_MarksStatusActive()
    {
        var ct = TestContext.Current.CancellationToken;
        var orgId = await CreateOrg(ct);
        var created = await InviteUser(orgId, "To Reactivate", UserRole.Reader, ct);
        await fixture.Client.PostAsync($"/orgs/{orgId}/users/{created.Id}/deactivate", content: null, ct);

        var response = await fixture.Client.PostAsync($"/orgs/{orgId}/users/{created.Id}/reactivate", content: null, ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var reactivated = await response.Content.ReadFromJsonAsync<GetUserResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal(UserStatus.Active, reactivated!.Status);
    }

    [Fact]
    public async Task DeleteUser_CalledTwice_BothReturnNoContent()
    {
        var ct = TestContext.Current.CancellationToken;
        var orgId = await CreateOrg(ct);
        var created = await InviteUser(orgId, "To Delete", UserRole.Reader, ct);

        var firstDelete = await fixture.Client.DeleteAsync($"/orgs/{orgId}/users/{created.Id}", ct);
        var secondDelete = await fixture.Client.DeleteAsync($"/orgs/{orgId}/users/{created.Id}", ct);

        Assert.Equal(HttpStatusCode.NoContent, firstDelete.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, secondDelete.StatusCode);
    }

    [Fact]
    public async Task GetUser_SeededDirectlyViaSql_IsVisibleThroughApi()
    {
        var ct = TestContext.Current.CancellationToken;
        var orgId = await CreateOrg(ct);
        var id = await UserSeeder.InsertUser(fixture.DbConnectionString, orgId, "Sql Seeded", $"{Guid.NewGuid()}@example.com", UserStatus.Active, UserRole.Admin, ct);

        var response = await fixture.Client.GetAsync($"/orgs/{orgId}/users/{id}", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var fetched = await response.Content.ReadFromJsonAsync<GetUserResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal(UserRole.Admin, fetched!.Role);
    }
}
