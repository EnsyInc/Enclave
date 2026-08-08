using System.Net;
using System.Net.Http.Json;

using EnsyInc.Enclave.ServiceTests.Fixtures;
using EnsyInc.Enclave.ServiceTests.Models;

namespace EnsyInc.Enclave.ServiceTests.Products;

[Collection(ApiCollectionDefinition.Name)]
public sealed class UpdateProductTests(ApiFixture fixture) : ProductsApiTestBase(fixture)
{
    [Fact]
    public async Task UpdateProduct_ValidBody_ReflectsInSubsequentGet()
    {
        var ct = TestContext.Current.CancellationToken;
        var created = await CreateProduct($"Before-Update-{Guid.NewGuid()}", ProductStatus.Active, ct);
        var newName = $"After-Update-{Guid.NewGuid()}";

        var putResponse = await Fixture.Client.PutAsJsonAsync($"/products/{created.Id}", new UpdateProductRequest(newName, "Updated description", ProductStatus.Active), ct);
        Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);

        var getResponse = await Fixture.Client.GetAsync($"/products/{created.Id}", ct);
        var fetched = await getResponse.Content.ReadFromJsonAsync<GetProductResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal(newName, fetched!.Name);
    }

    [Fact]
    public async Task UpdateProduct_NonexistentId_ReturnsNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var response = await Fixture.Client.PutAsJsonAsync($"/products/{Guid.NewGuid()}", new UpdateProductRequest("Ghost", null, ProductStatus.Active), ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("ProductNotFound", error!.ErrorCode);
    }
}
