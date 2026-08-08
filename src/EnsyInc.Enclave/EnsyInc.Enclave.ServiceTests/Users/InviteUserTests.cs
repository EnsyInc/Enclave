using System.Net;
using System.Net.Http.Json;

using EnsyInc.Enclave.ServiceTests.Fixtures;
using EnsyInc.Enclave.ServiceTests.Models;

namespace EnsyInc.Enclave.ServiceTests.Users;

[Collection(ApiCollectionDefinition.Name)]
public sealed class InviteUserTests(ApiFixture fixture) : UsersApiTestBase(fixture)
{
    [Fact]
    public async Task InviteUser_ValidBody_ReturnsCreatedWithInviteSentStatusAndLocationRoundTrips()
    {
        var ct = TestContext.Current.CancellationToken;
        var orgId = await CreateOrg(ct);
        var email = $"{Guid.NewGuid()}@example.com";

        var response = await Fixture.Client.PostAsJsonAsync($"/orgs/{orgId}/users", new InviteUserRequest("Jane Doe", email, UserRole.Admin), ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<GetUserResponse>(ApiFixture.JsonOptions, ct);
        Assert.NotNull(created);
        Assert.Equal(email, created!.Email);
        Assert.Equal(UserStatus.InviteSent, created.Status);
        Assert.Equal(orgId, created.OrgId);

        Assert.NotNull(response.Headers.Location);
        var getResponse = await Fixture.Client.GetAsync(response.Headers.Location, ct);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var fetched = await getResponse.Content.ReadFromJsonAsync<GetUserResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal(created.Id, fetched!.Id);
    }

    [Fact]
    public async Task InviteUser_InvalidEmail_ReturnsBadRequestWithValidationError()
    {
        var ct = TestContext.Current.CancellationToken;
        var orgId = await CreateOrg(ct);

        var response = await Fixture.Client.PostAsJsonAsync($"/orgs/{orgId}/users", new InviteUserRequest("Jane Doe", "not-an-email", UserRole.Reader), ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("ValidationError", error!.ErrorCode);
    }

    [Fact]
    public async Task InviteUser_NonexistentOrgId_ReturnsNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var response = await Fixture.Client.PostAsJsonAsync($"/orgs/{Guid.NewGuid()}/users", new InviteUserRequest("Jane Doe", $"{Guid.NewGuid()}@example.com", UserRole.Reader), ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("OrgNotFound", error!.ErrorCode);
    }
}
