using System.Net;
using System.Net.Http.Json;

using EnsyInc.Enclave.ServiceTests.Fixtures;
using EnsyInc.Enclave.ServiceTests.Models;

namespace EnsyInc.Enclave.ServiceTests.Products;

[Collection(ApiCollectionDefinition.Name)]
public sealed class RetireProductTests(ApiFixture fixture) : ProductsApiTestBase(fixture)
{
    [Fact]
    public async Task RetireProduct_MarksStatusRetired()
    {
        var ct = TestContext.Current.CancellationToken;
        var created = await CreateProduct($"To-Retire-{Guid.NewGuid()}", ProductStatus.Active, ct);

        var retireResponse = await Fixture.Client.PostAsync($"/products/{created.Id}/retire", content: null, ct);
        Assert.Equal(HttpStatusCode.OK, retireResponse.StatusCode);
        var retired = await retireResponse.Content.ReadFromJsonAsync<GetProductResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal(ProductStatus.Retired, retired!.Status);
    }

    [Fact]
    public async Task RetireProduct_NonexistentId_ReturnsNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var response = await Fixture.Client.PostAsync($"/products/{Guid.NewGuid()}/retire", content: null, ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("ProductNotFound", error!.ErrorCode);
    }
}
