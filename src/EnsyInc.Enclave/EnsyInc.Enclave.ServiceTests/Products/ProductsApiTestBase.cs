using System.Collections.ObjectModel;
using System.Net.Http.Json;

using EnsyInc.Enclave.ServiceTests.Fixtures;
using EnsyInc.Enclave.ServiceTests.Models;

namespace EnsyInc.Enclave.ServiceTests.Products;

/// <summary>Shared product setup and cleanup for the per-endpoint Product test classes.</summary>
public abstract class ProductsApiTestBase(ApiFixture fixture) : IAsyncDisposable
{
    protected ApiFixture Fixture { get; } = fixture;

    protected Collection<Guid> CreatedProductIds { get; } = [];

    public async ValueTask DisposeAsync()
    {
        var ct = TestContext.Current.CancellationToken;
        foreach (var id in CreatedProductIds)
        {
            await Fixture.Client.DeleteAsync($"/products/{id}", ct);
        }
    }

    protected async Task<GetProductResponse> CreateProduct(string name, ProductStatus status, CancellationToken ct)
    {
        var response = await Fixture.Client.PostAsJsonAsync("/products", new CreateProductRequest(name, "Seeded by ServiceTests", status), ct);
        response.EnsureSuccessStatusCode();
        var body = (await response.Content.ReadFromJsonAsync<GetProductResponse>(ApiFixture.JsonOptions, ct))!;
        CreatedProductIds.Add(body.Id);
        return body;
    }
}
