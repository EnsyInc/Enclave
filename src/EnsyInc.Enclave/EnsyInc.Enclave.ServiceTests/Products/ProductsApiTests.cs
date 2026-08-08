using System.Net;
using System.Net.Http.Json;

using EnsyInc.Enclave.ServiceTests.Fixtures;
using EnsyInc.Enclave.ServiceTests.Helpers;
using EnsyInc.Enclave.ServiceTests.Models;

namespace EnsyInc.Enclave.ServiceTests.Products;

[Collection(ApiCollectionDefinition.Name)]
public sealed class ProductsApiTests(ApiFixture fixture) : IAsyncDisposable
{
    private readonly List<Guid> _createdProductIds = [];

    public async ValueTask DisposeAsync()
    {
        var ct = TestContext.Current.CancellationToken;
        foreach (var id in _createdProductIds)
        {
            await fixture.Client.DeleteAsync($"/products/{id}", ct);
        }
    }

    private async Task<GetProductResponse> CreateProduct(string name, ProductStatus status, CancellationToken ct)
    {
        var response = await fixture.Client.PostAsJsonAsync("/products", new CreateProductRequest(name, "Seeded by ServiceTests", status), ct);
        response.EnsureSuccessStatusCode();
        var body = (await response.Content.ReadFromJsonAsync<GetProductResponse>(ApiFixture.JsonOptions, ct))!;
        _createdProductIds.Add(body.Id);
        return body;
    }

    [Fact]
    public async Task CreateProduct_ValidBody_ReturnsCreatedAndLocationRoundTrips()
    {
        var ct = TestContext.Current.CancellationToken;
        var name = $"Test-Product-{Guid.NewGuid()}";

        var response = await fixture.Client.PostAsJsonAsync("/products", new CreateProductRequest(name, "A test product", ProductStatus.Active), ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<GetProductResponse>(ApiFixture.JsonOptions, ct);
        Assert.NotNull(created);
        Assert.Equal(name, created!.Name);
        _createdProductIds.Add(created.Id);

        Assert.NotNull(response.Headers.Location);
        var getResponse = await fixture.Client.GetAsync(response.Headers.Location, ct);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var fetched = await getResponse.Content.ReadFromJsonAsync<GetProductResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal(created.Id, fetched!.Id);
    }

    [Fact]
    public async Task CreateProduct_EmptyName_ReturnsBadRequestWithValidationError()
    {
        var ct = TestContext.Current.CancellationToken;
        var response = await fixture.Client.PostAsJsonAsync("/products", new CreateProductRequest(string.Empty, null, ProductStatus.Active), ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("ValidationError", error!.ErrorCode);
    }

    [Fact]
    public async Task GetProduct_NonexistentId_ReturnsNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var response = await fixture.Client.GetAsync($"/products/{Guid.NewGuid()}", ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("ProductNotFound", error!.ErrorCode);
    }

    [Fact]
    public async Task GetProducts_FilteredByName_ReturnsOnlyMatchingProducts()
    {
        var ct = TestContext.Current.CancellationToken;
        var uniqueSuffix = Guid.NewGuid().ToString("N");
        var matchingName = $"Filter-Match-{uniqueSuffix}";
        await CreateProduct(matchingName, ProductStatus.Active, ct);
        await CreateProduct($"Filter-Other-{uniqueSuffix}", ProductStatus.Active, ct);

        var response = await fixture.Client.GetAsync($"/products?name=Filter-Match-{uniqueSuffix}", ct);

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

        var response = await fixture.Client.GetAsync("/products", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GetProductsResponse>(ApiFixture.JsonOptions, ct);
        Assert.Contains(body!.Products, p => p.Id == created.Id);
    }

    [Fact]
    public async Task UpdateProduct_ValidBody_ReflectsInSubsequentGet()
    {
        var ct = TestContext.Current.CancellationToken;
        var created = await CreateProduct($"Before-Update-{Guid.NewGuid()}", ProductStatus.Active, ct);
        var newName = $"After-Update-{Guid.NewGuid()}";

        var putResponse = await fixture.Client.PutAsJsonAsync($"/products/{created.Id}", new UpdateProductRequest(newName, "Updated description", ProductStatus.Active), ct);
        Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);

        var getResponse = await fixture.Client.GetAsync($"/products/{created.Id}", ct);
        var fetched = await getResponse.Content.ReadFromJsonAsync<GetProductResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal(newName, fetched!.Name);
    }

    [Fact]
    public async Task UpdateProduct_NonexistentId_ReturnsNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var response = await fixture.Client.PutAsJsonAsync($"/products/{Guid.NewGuid()}", new UpdateProductRequest("Ghost", null, ProductStatus.Active), ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("ProductNotFound", error!.ErrorCode);
    }

    [Fact]
    public async Task RetireProduct_MarksStatusRetired()
    {
        var ct = TestContext.Current.CancellationToken;
        var created = await CreateProduct($"To-Retire-{Guid.NewGuid()}", ProductStatus.Active, ct);

        var retireResponse = await fixture.Client.PostAsync($"/products/{created.Id}/retire", content: null, ct);
        Assert.Equal(HttpStatusCode.OK, retireResponse.StatusCode);
        var retired = await retireResponse.Content.ReadFromJsonAsync<GetProductResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal(ProductStatus.Retired, retired!.Status);
    }

    [Fact]
    public async Task RetireProduct_NonexistentId_ReturnsNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var response = await fixture.Client.PostAsync($"/products/{Guid.NewGuid()}/retire", content: null, ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("ProductNotFound", error!.ErrorCode);
    }

    [Fact]
    public async Task DeleteProduct_CalledTwice_BothReturnNoContent()
    {
        var ct = TestContext.Current.CancellationToken;
        var created = await CreateProduct($"To-Delete-{Guid.NewGuid()}", ProductStatus.Active, ct);

        var firstDelete = await fixture.Client.DeleteAsync($"/products/{created.Id}", ct);
        var secondDelete = await fixture.Client.DeleteAsync($"/products/{created.Id}", ct);

        Assert.Equal(HttpStatusCode.NoContent, firstDelete.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, secondDelete.StatusCode);
    }

    [Fact]
    public async Task GetProduct_SeededDirectlyViaSql_IsVisibleThroughApi()
    {
        var ct = TestContext.Current.CancellationToken;
        var id = await ProductSeeder.InsertProduct(fixture.DbConnectionString, $"Sql-Seeded-{Guid.NewGuid()}", "Seeded directly via SQL", ProductStatus.Upcoming, ct);
        _createdProductIds.Add(id);

        var response = await fixture.Client.GetAsync($"/products/{id}", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var fetched = await response.Content.ReadFromJsonAsync<GetProductResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal(ProductStatus.Upcoming, fetched!.Status);
    }
}
