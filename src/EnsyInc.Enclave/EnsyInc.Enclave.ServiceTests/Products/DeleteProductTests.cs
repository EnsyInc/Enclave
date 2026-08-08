using System.Net;

using EnsyInc.Enclave.ServiceTests.Fixtures;
using EnsyInc.Enclave.ServiceTests.Models;

namespace EnsyInc.Enclave.ServiceTests.Products;

[Collection(ApiCollectionDefinition.Name)]
public sealed class DeleteProductTests(ApiFixture fixture) : ProductsApiTestBase(fixture)
{
    [Fact]
    public async Task DeleteProduct_CalledTwice_BothReturnNoContent()
    {
        var ct = TestContext.Current.CancellationToken;
        var created = await CreateProduct($"To-Delete-{Guid.NewGuid()}", ProductStatus.Active, ct);

        var firstDelete = await Fixture.Client.DeleteAsync($"/products/{created.Id}", ct);
        var secondDelete = await Fixture.Client.DeleteAsync($"/products/{created.Id}", ct);

        Assert.Equal(HttpStatusCode.NoContent, firstDelete.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, secondDelete.StatusCode);
    }
}
