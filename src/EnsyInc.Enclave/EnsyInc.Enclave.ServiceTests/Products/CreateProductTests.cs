using System.Net;
using System.Net.Http.Json;

using EnsyInc.Enclave.ServiceTests.Fixtures;
using EnsyInc.Enclave.ServiceTests.Models;

namespace EnsyInc.Enclave.ServiceTests.Products;

[Collection(ApiCollectionDefinition.Name)]
public sealed class CreateProductTests(ApiFixture fixture) : ProductsApiTestBase(fixture)
{
    [Fact]
    public async Task CreateProduct_ValidBody_ReturnsCreatedAndLocationRoundTrips()
    {
        var ct = TestContext.Current.CancellationToken;
        var name = $"Test-Product-{Guid.NewGuid()}";

        var response = await Fixture.Client.PostAsJsonAsync("/products", new CreateProductRequest(name, "A test product", ProductStatus.Active), ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<GetProductResponse>(ApiFixture.JsonOptions, ct);
        Assert.NotNull(created);
        Assert.Equal(name, created!.Name);
        CreatedProductIds.Add(created.Id);

        Assert.NotNull(response.Headers.Location);
        var getResponse = await Fixture.Client.GetAsync(response.Headers.Location, ct);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var fetched = await getResponse.Content.ReadFromJsonAsync<GetProductResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal(created.Id, fetched!.Id);
    }

    [Fact]
    public async Task CreateProduct_EmptyName_ReturnsBadRequestWithValidationError()
    {
        var ct = TestContext.Current.CancellationToken;
        var response = await Fixture.Client.PostAsJsonAsync("/products", new CreateProductRequest(string.Empty, null, ProductStatus.Active), ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("ValidationError", error!.ErrorCode);
    }
}
