using System.Net;
using System.Net.Http.Json;

using EnsyInc.Enclave.ServiceTests.Fixtures;
using EnsyInc.Enclave.ServiceTests.Helpers;
using EnsyInc.Enclave.ServiceTests.Models;

namespace EnsyInc.Enclave.ServiceTests.Users;

[Collection(ApiCollectionDefinition.Name)]
public sealed class GetUserTests(ApiFixture fixture) : UsersApiTestBase(fixture)
{
    [Fact]
    public async Task GetUser_UserFromDifferentOrg_ReturnsNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var orgA = await CreateOrg(ct);
        var orgB = await CreateOrg(ct);
        var userInOrgA = await InviteUser(orgA, "Org A User", UserRole.Reader, ct);

        var response = await Fixture.Client.GetAsync($"/orgs/{orgB}/users/{userInOrgA.Id}", ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("UserNotFound", error!.ErrorCode);
    }

    [Fact]
    public async Task GetUser_SeededDirectlyViaSql_IsVisibleThroughApi()
    {
        var ct = TestContext.Current.CancellationToken;
        var orgId = await CreateOrg(ct);
        var id = await UserSeeder.InsertUser(Fixture.DbConnectionString, orgId, "Sql Seeded", $"{Guid.NewGuid()}@example.com", UserStatus.Active, UserRole.Admin, ct);

        var response = await Fixture.Client.GetAsync($"/orgs/{orgId}/users/{id}", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var fetched = await response.Content.ReadFromJsonAsync<GetUserResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal(UserRole.Admin, fetched!.Role);
    }
}
