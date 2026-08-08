using System.Collections.ObjectModel;
using System.Net.Http.Json;

using EnsyInc.Enclave.ServiceTests.Fixtures;
using EnsyInc.Enclave.ServiceTests.Models;

namespace EnsyInc.Enclave.ServiceTests.Users;

/// <summary>Shared org/user setup and cleanup for the per-endpoint User test classes.</summary>
public abstract class UsersApiTestBase(ApiFixture fixture) : IAsyncDisposable
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
    }

    protected async Task<Guid> CreateOrg(CancellationToken ct)
    {
        var response = await Fixture.Client.PostAsJsonAsync("/orgs", new CreateOrgRequest($"Test-Org-{Guid.NewGuid()}"), ct);
        response.EnsureSuccessStatusCode();
        var body = (await response.Content.ReadFromJsonAsync<GetOrgResponse>(ApiFixture.JsonOptions, ct))!;
        CreatedOrgIds.Add(body.Id);
        return body.Id;
    }

    protected async Task<GetUserResponse> InviteUser(Guid orgId, string name, UserRole role, CancellationToken ct)
    {
        var response = await Fixture.Client.PostAsJsonAsync($"/orgs/{orgId}/users", new InviteUserRequest(name, $"{Guid.NewGuid()}@example.com", role), ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<GetUserResponse>(ApiFixture.JsonOptions, ct))!;
    }
}
