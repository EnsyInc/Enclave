using System.Net;

using EnsyInc.Enclave.ServiceTests.Fixtures;

namespace EnsyInc.Enclave.ServiceTests.Licenses;

[Collection(ApiCollectionDefinition.Name)]
public sealed class DeleteLicenseTests(ApiFixture fixture) : LicensesApiTestBase(fixture)
{
    [Fact]
    public async Task DeleteLicense_CalledTwice_BothReturnNoContent()
    {
        var ct = TestContext.Current.CancellationToken;
        var orgId = await CreateOrg(ct);
        var productId = await CreateProduct(ct);
        var created = await GrantLicense(orgId, productId, DateTime.UtcNow, DateTime.UtcNow.AddYears(1), ct);

        var firstDelete = await Fixture.Client.DeleteAsync($"/licenses/{created.Id}", ct);
        var secondDelete = await Fixture.Client.DeleteAsync($"/licenses/{created.Id}", ct);

        Assert.Equal(HttpStatusCode.NoContent, firstDelete.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, secondDelete.StatusCode);
    }
}
