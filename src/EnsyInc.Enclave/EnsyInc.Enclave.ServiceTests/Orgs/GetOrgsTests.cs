using System.Net;
using System.Net.Http.Json;

using EnsyInc.Enclave.ServiceTests.Fixtures;
using EnsyInc.Enclave.ServiceTests.Models;

namespace EnsyInc.Enclave.ServiceTests.Orgs;

[Collection(ApiCollectionDefinition.Name)]
public sealed class GetOrgsTests(ApiFixture fixture) : OrgsApiTestBase(fixture)
{
    [Fact]
    public async Task GetOrgs_FilteredByName_ReturnsOnlyMatchingOrgs()
    {
        var ct = TestContext.Current.CancellationToken;
        var uniqueSuffix = Guid.NewGuid().ToString("N");
        var matchingName = $"Filter-Match-{uniqueSuffix}";
        await CreateOrg(matchingName, ct);
        await CreateOrg($"Filter-Other-{uniqueSuffix}", ct);

        var response = await Fixture.Client.GetAsync($"/orgs?name=Filter-Match-{uniqueSuffix}", ct);

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

        var response = await Fixture.Client.GetAsync("/orgs", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GetOrgsResponse>(ApiFixture.JsonOptions, ct);
        Assert.Contains(body!.Orgs, o => o.Id == created.Id);
    }
}
