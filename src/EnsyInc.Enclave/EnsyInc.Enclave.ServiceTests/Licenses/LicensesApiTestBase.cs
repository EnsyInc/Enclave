using System.Collections.ObjectModel;
using System.Net.Http.Json;

using EnsyInc.Enclave.ServiceTests.Fixtures;
using EnsyInc.Enclave.ServiceTests.Models;

namespace EnsyInc.Enclave.ServiceTests.Licenses;

/// <summary>Shared org/product/license setup and cleanup for the per-endpoint License test classes.</summary>
public abstract class LicensesApiTestBase(ApiFixture fixture) : IAsyncDisposable
{
    private readonly List<Guid> _createdOrgIds = [];
    private readonly List<Guid> _createdProductIds = [];

    protected ApiFixture Fixture { get; } = fixture;

    protected Collection<Guid> CreatedLicenseIds { get; } = [];

    public async ValueTask DisposeAsync()
    {
        var ct = TestContext.Current.CancellationToken;
        foreach (var id in CreatedLicenseIds)
        {
            await Fixture.Client.DeleteAsync($"/licenses/{id}", ct);
        }

        foreach (var id in _createdProductIds)
        {
            await Fixture.Client.DeleteAsync($"/products/{id}", ct);
        }

        foreach (var id in _createdOrgIds)
        {
            await Fixture.Client.DeleteAsync($"/orgs/{id}", ct);
        }

        GC.SuppressFinalize(this);
    }

    protected async Task<Guid> CreateOrg(CancellationToken ct)
    {
        var response = await Fixture.Client.PostAsJsonAsync("/orgs", new CreateOrgRequest($"Test-Org-{Guid.NewGuid()}"), ct);
        response.EnsureSuccessStatusCode();
        var body = (await response.Content.ReadFromJsonAsync<GetOrgResponse>(ApiFixture.JsonOptions, ct))!;
        _createdOrgIds.Add(body.Id);
        return body.Id;
    }

    protected async Task<Guid> CreateProduct(CancellationToken ct)
    {
        var response = await Fixture.Client.PostAsJsonAsync("/products", new CreateProductRequest($"Test-Product-{Guid.NewGuid()}", null, ProductStatus.Active), ct);
        response.EnsureSuccessStatusCode();
        var body = (await response.Content.ReadFromJsonAsync<GetProductResponse>(ApiFixture.JsonOptions, ct))!;
        _createdProductIds.Add(body.Id);
        return body.Id;
    }

    protected async Task<GetLicenseResponse> GrantLicense(Guid orgId, Guid productId, DateTime start, DateTime end, CancellationToken ct)
    {
        var response = await Fixture.Client.PostAsJsonAsync("/licenses", new GrantLicenseRequest(orgId, productId, start, end), ct);
        response.EnsureSuccessStatusCode();
        var body = (await response.Content.ReadFromJsonAsync<GetLicenseResponse>(ApiFixture.JsonOptions, ct))!;
        CreatedLicenseIds.Add(body.Id);
        return body;
    }
}
