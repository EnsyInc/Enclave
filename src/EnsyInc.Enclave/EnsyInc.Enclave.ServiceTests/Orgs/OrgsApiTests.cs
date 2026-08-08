using System.Net;
using System.Net.Http.Json;

using EnsyInc.Enclave.ServiceTests.Fixtures;
using EnsyInc.Enclave.ServiceTests.Helpers;
using EnsyInc.Enclave.ServiceTests.Models;

namespace EnsyInc.Enclave.ServiceTests.Orgs;

[Collection(ApiCollectionDefinition.Name)]
public sealed class OrgsApiTests(ApiFixture fixture) : IAsyncDisposable
{
    private readonly List<Guid> _createdOrgIds = [];

    public async ValueTask DisposeAsync()
    {
        var ct = TestContext.Current.CancellationToken;
        foreach (var id in _createdOrgIds)
        {
            await fixture.Client.DeleteAsync($"/orgs/{id}", ct);
        }
    }

    private async Task<GetOrgResponse> CreateOrg(string name, CancellationToken ct)
    {
        var response = await fixture.Client.PostAsJsonAsync("/orgs", new CreateOrgRequest(name), ct);
        response.EnsureSuccessStatusCode();
        var body = (await response.Content.ReadFromJsonAsync<GetOrgResponse>(ApiFixture.JsonOptions, ct))!;
        _createdOrgIds.Add(body.Id);
        return body;
    }

    [Fact]
    public async Task CreateOrg_ValidBody_ReturnsCreatedAndLocationRoundTrips()
    {
        var ct = TestContext.Current.CancellationToken;
        var name = $"Test-Org-{Guid.NewGuid()}";

        var response = await fixture.Client.PostAsJsonAsync("/orgs", new CreateOrgRequest(name), ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<GetOrgResponse>(ApiFixture.JsonOptions, ct);
        Assert.NotNull(created);
        Assert.Equal(name, created!.Name);
        Assert.Equal(OrgStatus.Active, created.Status);
        _createdOrgIds.Add(created.Id);

        Assert.NotNull(response.Headers.Location);
        var getResponse = await fixture.Client.GetAsync(response.Headers.Location, ct);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var fetched = await getResponse.Content.ReadFromJsonAsync<GetOrgResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal(created.Id, fetched!.Id);
    }

    [Fact]
    public async Task CreateOrg_EmptyName_ReturnsBadRequestWithValidationError()
    {
        var ct = TestContext.Current.CancellationToken;
        var response = await fixture.Client.PostAsJsonAsync("/orgs", new CreateOrgRequest(string.Empty), ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("ValidationError", error!.ErrorCode);
    }

    [Fact]
    public async Task GetOrg_NonexistentId_ReturnsNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var response = await fixture.Client.GetAsync($"/orgs/{Guid.NewGuid()}", ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("OrgNotFound", error!.ErrorCode);
    }

    [Fact]
    public async Task GetOrgs_FilteredByName_ReturnsOnlyMatchingOrgs()
    {
        var ct = TestContext.Current.CancellationToken;
        var uniqueSuffix = Guid.NewGuid().ToString("N");
        var matchingName = $"Filter-Match-{uniqueSuffix}";
        await CreateOrg(matchingName, ct);
        await CreateOrg($"Filter-Other-{uniqueSuffix}", ct);

        var response = await fixture.Client.GetAsync($"/orgs?name=Filter-Match-{uniqueSuffix}", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GetOrgsResponse>(ApiFixture.JsonOptions, ct);
        var match = Assert.Single(body!.Orgs);
        Assert.Equal(matchingName, match.Name);
    }

    [Fact]
    public async Task GetOrgs_NoFilter_IncludesSeededOrg()
    {
        var ct = TestContext.Current.CancellationToken;
        var created = await CreateOrg($"Unfiltered-{Guid.NewGuid()}", ct);

        var response = await fixture.Client.GetAsync("/orgs", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GetOrgsResponse>(ApiFixture.JsonOptions, ct);
        Assert.Contains(body!.Orgs, o => o.Id == created.Id);
    }

    [Fact]
    public async Task UpdateOrg_ValidBody_ReflectsInSubsequentGet()
    {
        var ct = TestContext.Current.CancellationToken;
        var created = await CreateOrg($"Before-Update-{Guid.NewGuid()}", ct);
        var newName = $"After-Update-{Guid.NewGuid()}";

        var putResponse = await fixture.Client.PutAsJsonAsync($"/orgs/{created.Id}", new UpdateOrgRequest(newName), ct);
        Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);

        var getResponse = await fixture.Client.GetAsync($"/orgs/{created.Id}", ct);
        var fetched = await getResponse.Content.ReadFromJsonAsync<GetOrgResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal(newName, fetched!.Name);
    }

    [Fact]
    public async Task UpdateOrg_NonexistentId_ReturnsNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var response = await fixture.Client.PutAsJsonAsync($"/orgs/{Guid.NewGuid()}", new UpdateOrgRequest("Ghost"), ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("OrgNotFound", error!.ErrorCode);
    }

    [Fact]
    public async Task DeactivateOrg_MarksStatusDeactivated()
    {
        var ct = TestContext.Current.CancellationToken;
        var created = await CreateOrg($"To-Deactivate-{Guid.NewGuid()}", ct);

        var response = await fixture.Client.PostAsync($"/orgs/{created.Id}/deactivate", content: null, ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var deactivated = await response.Content.ReadFromJsonAsync<GetOrgResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal(OrgStatus.Deactivated, deactivated!.Status);
    }

    [Fact]
    public async Task DeactivateOrg_NonexistentId_ReturnsNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var response = await fixture.Client.PostAsync($"/orgs/{Guid.NewGuid()}/deactivate", content: null, ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("OrgNotFound", error!.ErrorCode);
    }

    [Fact]
    public async Task ReactivateOrg_AfterDeactivate_MarksStatusActive()
    {
        var ct = TestContext.Current.CancellationToken;
        var created = await CreateOrg($"To-Reactivate-{Guid.NewGuid()}", ct);
        await fixture.Client.PostAsync($"/orgs/{created.Id}/deactivate", content: null, ct);

        var response = await fixture.Client.PostAsync($"/orgs/{created.Id}/reactivate", content: null, ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var reactivated = await response.Content.ReadFromJsonAsync<GetOrgResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal(OrgStatus.Active, reactivated!.Status);
    }

    [Fact]
    public async Task DeleteOrg_CalledTwice_BothReturnNoContent()
    {
        var ct = TestContext.Current.CancellationToken;
        var created = await CreateOrg($"To-Delete-{Guid.NewGuid()}", ct);

        var firstDelete = await fixture.Client.DeleteAsync($"/orgs/{created.Id}", ct);
        var secondDelete = await fixture.Client.DeleteAsync($"/orgs/{created.Id}", ct);

        Assert.Equal(HttpStatusCode.NoContent, firstDelete.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, secondDelete.StatusCode);
    }

    [Fact]
    public async Task GetOrg_SeededDirectlyViaSql_IsVisibleThroughApi()
    {
        var ct = TestContext.Current.CancellationToken;
        var id = await OrgSeeder.InsertOrg(fixture.DbConnectionString, $"Sql-Seeded-{Guid.NewGuid()}", OrgStatus.Deactivated, ct);
        _createdOrgIds.Add(id);

        var response = await fixture.Client.GetAsync($"/orgs/{id}", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var fetched = await response.Content.ReadFromJsonAsync<GetOrgResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal(OrgStatus.Deactivated, fetched!.Status);
    }
}
