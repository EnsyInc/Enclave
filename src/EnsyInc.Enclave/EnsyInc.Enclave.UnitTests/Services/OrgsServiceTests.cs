using System.Linq.Expressions;

using EnsyInc.Enclave.Core.Errors;
using EnsyInc.Enclave.Core.Models;
using EnsyInc.Enclave.DataAccess.Abstractions;
using EnsyInc.Enclave.DataAccess.Mappers;
using EnsyInc.Enclave.DataAccess.Models;
using EnsyInc.Enclave.Services.Implementations;

using EnsyNet.Core.Results;
using EnsyNet.DataAccess.Abstractions.Errors;
using EnsyNet.DataAccess.Abstractions.Models;

using Moq;

namespace EnsyInc.Enclave.UnitTests.Services;

// Only covers scenarios ServiceTests (black-box, against a real running instance) can't reach:
// repo/DB failures that can't be provoked through the API, and internal call-sequencing
// assertions only visible via mock Verify. DeactivateOrg/ReactivateOrg share the exact same
// fetch-then-update code path (just a different status value); it's exercised fully once
// (via Deactivate) rather than duplicated for Reactivate.
public sealed class OrgsServiceTests
{
    private readonly Mock<IOrgRepo> _orgRepoMock = new();
    private readonly OrgsService _sut;

    public OrgsServiceTests()
    {
        _sut = new OrgsService(_orgRepoMock.Object);
    }

    private static OrgEntity CreateEntity(Guid? id = null, string name = "Acme", OrgStatus status = OrgStatus.Active)
        => new()
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Status = status,
        };

    [Fact]
    public async Task ListOrgs_RepoFails_ReturnsUnexpectedError()
    {
        _orgRepoMock.Setup(r => r.GetAll(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError<IEnumerable<OrgEntity>>(new EntityNotFoundError<OrgEntity>()));

        var result = await _sut.ListOrgs(CancellationToken.None);

        Assert.True(result.HasError);
        Assert.IsType<UnexpectedError>(result.Error);
    }

    [Fact]
    public async Task ListOrgsByName_RepoFails_ReturnsUnexpectedError()
    {
        _orgRepoMock.Setup(r => r.GetManyByExpression(It.IsAny<Expression<Func<OrgEntity, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError<IEnumerable<OrgEntity>>(new EntityNotFoundError<OrgEntity>()));

        var result = await _sut.ListOrgsByName("Acme", CancellationToken.None);

        Assert.True(result.HasError);
        Assert.IsType<UnexpectedError>(result.Error);
    }

    [Fact]
    public async Task GetOrg_RepoReturnsOtherError_ReturnsUnexpectedError()
    {
        var id = Guid.NewGuid();
        _orgRepoMock.Setup(r => r.GetById(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError<OrgEntity>(new UpdateOperationFailedError()));

        var result = await _sut.GetOrg(id, CancellationToken.None);

        Assert.True(result.HasError);
        Assert.IsType<UnexpectedError>(result.Error);
    }

    [Fact]
    public async Task CreateOrg_RepoReturnsNotFound_ReturnsOrgNotFoundError()
    {
        var org = CreateEntity().ToCoreModel();
        _orgRepoMock.Setup(r => r.Insert(It.IsAny<OrgEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError<OrgEntity>(new EntityNotFoundError<OrgEntity>()));

        var result = await _sut.CreateOrg(org);

        Assert.True(result.HasError);
        Assert.IsType<OrgNotFoundError>(result.Error);
    }

    [Fact]
    public async Task CreateOrg_RepoReturnsOtherError_ReturnsUnexpectedError()
    {
        var org = CreateEntity().ToCoreModel();
        _orgRepoMock.Setup(r => r.Insert(It.IsAny<OrgEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError<OrgEntity>(new UpdateOperationFailedError()));

        var result = await _sut.CreateOrg(org);

        Assert.True(result.HasError);
        Assert.IsType<UnexpectedError>(result.Error);
    }

    [Fact]
    public async Task UpdateOrg_UpdateFailsAfterSuccessfulPrecheck_ReturnsOrgNotFoundError()
    {
        var entity = CreateEntity();
        var org = entity.ToCoreModel();
        _orgRepoMock.Setup(r => r.GetById(org.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(entity));
        _orgRepoMock.Setup(r => r.Update(org.Id, It.IsAny<Action<EntityUpdates<OrgEntity>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError(new UpdateOperationFailedError()));

        var result = await _sut.UpdateOrg(org, CancellationToken.None);

        Assert.True(result.HasError);
        Assert.IsType<OrgNotFoundError>(result.Error);
    }

    [Fact]
    public async Task DeactivateOrg_UpdateFailsAfterSuccessfulPrecheck_ReturnsOrgNotFoundError()
    {
        var id = Guid.NewGuid();
        _orgRepoMock.Setup(r => r.GetById(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(CreateEntity(id)));
        _orgRepoMock.Setup(r => r.Update(id, It.IsAny<Action<EntityUpdates<OrgEntity>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError(new UpdateOperationFailedError()));

        var result = await _sut.DeactivateOrg(id, CancellationToken.None);

        Assert.True(result.HasError);
        Assert.IsType<OrgNotFoundError>(result.Error);
    }

    [Fact]
    public async Task SoftDeleteOrg_RepoReturnsOtherError_ReturnsUnexpectedError()
    {
        var id = Guid.NewGuid();
        _orgRepoMock.Setup(r => r.SoftDelete(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError(new EntityNotFoundError<OrgEntity>()));

        var result = await _sut.SoftDeleteOrg(id, CancellationToken.None);

        Assert.True(result.HasError);
        Assert.IsType<UnexpectedError>(result.Error);
    }
}
