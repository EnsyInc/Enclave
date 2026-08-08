using System.Collections.ObjectModel;
using System.Net.Http.Json;

using EnsyInc.Enclave.ServiceTests.Fixtures;
using EnsyInc.Enclave.ServiceTests.Models;

namespace EnsyInc.Enclave.ServiceTests.Orgs;

/// <summary>Shared org setup and cleanup for the per-endpoint Org test classes.</summary>
public abstract class OrgsApiTestBase(ApiFixture fixture) : IAsyncDisposable
{
    protected ApiFixture Fixture { get; } = fixture;

    protected Collection<Guid> CreatedOrgIds { get; } = [];

    public async ValueTask DisposeAsync()
    {
        var ct = TestContext.Current.CancellationToken;
        foreach (var id in CreatedOrgIds)
        {
            await Fixture.Client.DeleteAsync($"/orgs/{id}", ct);
        }

        GC.SuppressFinalize(this);
    }

    protected async Task<GetOrgResponse> CreateOrg(string name, CancellationToken ct)
    {
        var response = await Fixture.Client.PostAsJsonAsync("/orgs", new CreateOrgRequest(name), ct);
        response.EnsureSuccessStatusCode();
        var body = (await response.Content.ReadFromJsonAsync<GetOrgResponse>(ApiFixture.JsonOptions, ct))!;
        CreatedOrgIds.Add(body.Id);
        return body;
    }
}
