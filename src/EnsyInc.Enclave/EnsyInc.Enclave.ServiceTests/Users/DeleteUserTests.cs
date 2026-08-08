using System.Net;

using EnsyInc.Enclave.ServiceTests.Fixtures;
using EnsyInc.Enclave.ServiceTests.Models;

namespace EnsyInc.Enclave.ServiceTests.Users;

[Collection(ApiCollectionDefinition.Name)]
public sealed class DeleteUserTests(ApiFixture fixture) : UsersApiTestBase(fixture)
{
    [Fact]
    public async Task DeleteUser_CalledTwice_BothReturnNoContent()
    {
        var ct = TestContext.Current.CancellationToken;
        var orgId = await CreateOrg(ct);
        var created = await InviteUser(orgId, "To Delete", UserRole.Reader, ct);

        var firstDelete = await Fixture.Client.DeleteAsync($"/orgs/{orgId}/users/{created.Id}", ct);
        var secondDelete = await Fixture.Client.DeleteAsync($"/orgs/{orgId}/users/{created.Id}", ct);

        Assert.Equal(HttpStatusCode.NoContent, firstDelete.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, secondDelete.StatusCode);
    }
}
