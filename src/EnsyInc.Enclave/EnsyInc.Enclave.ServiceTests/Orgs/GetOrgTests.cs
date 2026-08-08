using System.Net;
using System.Net.Http.Json;

using EnsyInc.Enclave.ServiceTests.Fixtures;
using EnsyInc.Enclave.ServiceTests.Helpers;
using EnsyInc.Enclave.ServiceTests.Models;

namespace EnsyInc.Enclave.ServiceTests.Orgs;

[Collection(ApiCollectionDefinition.Name)]
public sealed class GetOrgTests(ApiFixture fixture) : OrgsApiTestBase(fixture)
{
    [Fact]
    public async Task GetOrg_NonexistentId_ReturnsNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var response = await Fixture.Client.GetAsync($"/orgs/{Guid.NewGuid()}", ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal("OrgNotFound", error!.ErrorCode);
    }

    [Fact]
    public async Task GetOrg_SeededDirectlyViaSql_IsVisibleThroughApi()
    {
        var ct = TestContext.Current.CancellationToken;
        var id = await OrgSeeder.InsertOrg(Fixture.DbConnectionString, $"Sql-Seeded-{Guid.NewGuid()}", OrgStatus.Deactivated, ct);
        CreatedOrgIds.Add(id);

        var response = await Fixture.Client.GetAsync($"/orgs/{id}", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var fetched = await response.Content.ReadFromJsonAsync<GetOrgResponse>(ApiFixture.JsonOptions, ct);
        Assert.Equal(OrgStatus.Deactivated, fetched!.Status);
    }
}
