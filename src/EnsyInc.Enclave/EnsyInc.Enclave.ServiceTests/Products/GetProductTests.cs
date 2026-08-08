using System.Net;
using System.Net.Http.Json;

using EnsyInc.Enclave.ServiceTests.Fixtures;
using EnsyInc.Enclave.ServiceTests.Helpers;
using EnsyInc.Enclave.ServiceTests.Models;

namespace EnsyInc.Enclave.ServiceTests.Products;

[Collection(ApiCollectionDefinition.Name)]
public sealed class GetProductTests(ApiFixture fixture) : ProductsApiTestBase(fixture)
{
    [Fact]
    public async Task GetProduct_NonexistentId_ReturnsNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var response = await Fixture.Client.GetAsync($"/products/{Guid.NewGuid()}", ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("ProductNotFound", error!.ErrorCode);
    }

    [Fact]
    public async Task GetProduct_SeededDirectlyViaSql_IsVisibleThroughApi()
    {
        var ct = TestContext.Current.CancellationToken;
        var id = await ProductSeeder.InsertProduct(Fixture.DbConnectionString, $"Sql-Seeded-{Guid.NewGuid()}", "Seeded directly via SQL", ProductStatus.Upcoming, ct);
        CreatedProductIds.Add(id);

        var response = await Fixture.Client.GetAsync($"/products/{id}", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var fetched = await response.Content.ReadFromJsonAsync<GetProductResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal(ProductStatus.Upcoming, fetched!.Status);
    }
}
