using System.Net;

using EnsyInc.Enclave.ServiceTests.Fixtures;

namespace EnsyInc.Enclave.ServiceTests.Orgs;

[Collection(ApiCollectionDefinition.Name)]
public sealed class DeleteOrgTests(ApiFixture fixture) : OrgsApiTestBase(fixture)
{
    [Fact]
    public async Task DeleteOrg_CalledTwice_BothReturnNoContent()
    {
        var ct = TestContext.Current.CancellationToken;
        var created = await CreateOrg($"To-Delete-{Guid.NewGuid()}", ct);

        var firstDelete = await Fixture.Client.DeleteAsync($"/orgs/{created.Id}", ct);
        var secondDelete = await Fixture.Client.DeleteAsync($"/orgs/{created.Id}", ct);

        Assert.Equal(HttpStatusCode.NoContent, firstDelete.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, secondDelete.StatusCode);
    }
}
