using System.Net;
using System.Net.Http.Json;

using EnsyInc.Enclave.ServiceTests.Fixtures;
using EnsyInc.Enclave.ServiceTests.Models;

namespace EnsyInc.Enclave.ServiceTests.Products;

[Collection(ApiCollectionDefinition.Name)]
public sealed class GetProductsTests(ApiFixture fixture) : ProductsApiTestBase(fixture)
{
    [Fact]
    public async Task GetProducts_FilteredByName_ReturnsOnlyMatchingProducts()
    {
        var ct = TestContext.Current.CancellationToken;
        var uniqueSuffix = Guid.NewGuid().ToString("N");
        var matchingName = $"Filter-Match-{uniqueSuffix}";
        await CreateProduct(matchingName, ProductStatus.Active, ct);
        await CreateProduct($"Filter-Other-{uniqueSuffix}", ProductStatus.Active, ct);

        var response = await Fixture.Client.GetAsync($"/products?name=Filter-Match-{uniqueSuffix}", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GetProductsResponse>(ApiFixture.JsonOptions, ct);
        var match = Assert.Single(body!.Products);
        Assert.Equal(matchingName, match.Name);
    }

    [Fact]
    public async Task GetProducts_NoFilter_IncludesSeededProduct()
    {
        var ct = TestContext.Current.CancellationToken;
        var created = await CreateProduct($"Unfiltered-{Guid.NewGuid()}", ProductStatus.Active, ct);

        var response = await Fixture.Client.GetAsync("/products", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GetProductsResponse>(ApiFixture.JsonOptions, ct);
        Assert.Contains(body!.Products, p => p.Id == created.Id);
    }
}
