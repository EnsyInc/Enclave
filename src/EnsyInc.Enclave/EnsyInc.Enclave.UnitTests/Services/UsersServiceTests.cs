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
// assertions only visible via mock Verify/SetupSequence. Org-not-found and cross-org 404s are
// fully reachable via real HTTP calls (bad orgId in the route, or a user id from a different
// org), so those live in ServiceTests instead. DeactivateUser/ReactivateUser share UpdateUser's
// exact fetch/update shape, so that shape is exercised fully once (via UpdateUser) rather than
// duplicated per action.
public sealed class UsersServiceTests
{
    private readonly Mock<IUserRepo> _userRepoMock = new();
    private readonly Mock<IOrgRepo> _orgRepoMock = new();
    private readonly UsersService _sut;

    public UsersServiceTests()
    {
        _sut = new UsersService(_userRepoMock.Object, _orgRepoMock.Object);
    }

    private static OrgEntity CreateOrgEntity(Guid? id = null)
        => new()
        {
            Id = id ?? Guid.NewGuid(),
            Name = "Acme",
            Status = OrgStatus.Active,
        };

    private static UserEntity CreateUserEntity(Guid orgId, Guid? id = null, string name = "Jane", UserStatus status = UserStatus.Active)
        => new()
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Email = "jane@example.com",
            OrgId = orgId,
            Status = status,
            Role = UserRole.Reader,
        };

    private void SetupOrgExists(Guid orgId)
        => _orgRepoMock.Setup(r => r.GetById(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(CreateOrgEntity(orgId)));

    [Fact]
    public async Task ListUsers_OrgRepoFails_ReturnsUnexpectedError()
    {
        var orgId = Guid.NewGuid();
        _orgRepoMock.Setup(r => r.GetById(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError<OrgEntity>(new UpdateOperationFailedError()));

        var result = await _sut.ListUsers(orgId, CancellationToken.None);

        Assert.True(result.HasError);
        Assert.IsType<UnexpectedError>(result.Error);
        _userRepoMock.Verify(r => r.GetManyByExpression(It.IsAny<Expression<Func<UserEntity, bool>>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ListUsers_UserRepoFails_ReturnsUnexpectedError()
    {
        var orgId = Guid.NewGuid();
        SetupOrgExists(orgId);
        _userRepoMock.Setup(r => r.GetManyByExpression(It.IsAny<Expression<Func<UserEntity, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError<IEnumerable<UserEntity>>(new EntityNotFoundError<UserEntity>()));

        var result = await _sut.ListUsers(orgId, CancellationToken.None);

        Assert.True(result.HasError);
        Assert.IsType<UnexpectedError>(result.Error);
    }

    [Fact]
    public async Task GetUser_UserRepoReturnsOtherError_ReturnsUnexpectedError()
    {
        var orgId = Guid.NewGuid();
        var id = Guid.NewGuid();
        SetupOrgExists(orgId);
        _userRepoMock.Setup(r => r.GetById(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError<UserEntity>(new UpdateOperationFailedError()));

        var result = await _sut.GetUser(orgId, id, CancellationToken.None);

        Assert.True(result.HasError);
        Assert.IsType<UnexpectedError>(result.Error);
    }

    [Fact]
    public async Task InviteUser_RepoReturnsOtherError_ReturnsUnexpectedError()
    {
        var orgId = Guid.NewGuid();
        SetupOrgExists(orgId);
        var user = CreateUserEntity(orgId).ToCoreModel();
        _userRepoMock.Setup(r => r.Insert(It.IsAny<UserEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError<UserEntity>(new UpdateOperationFailedError()));

        var result = await _sut.InviteUser(orgId, user, CancellationToken.None);

        Assert.True(result.HasError);
        Assert.IsType<UnexpectedError>(result.Error);
    }

    [Fact]
    public async Task InviteUsers_SecondInsertFails_StopsBatchAndReturnsUnexpectedError()
    {
        var orgId = Guid.NewGuid();
        SetupOrgExists(orgId);
        var users = new[]
        {
            CreateUserEntity(orgId, name: "First").ToCoreModel(),
            CreateUserEntity(orgId, name: "Second").ToCoreModel(),
            CreateUserEntity(orgId, name: "Third").ToCoreModel(),
        };
        _userRepoMock.SetupSequence(r => r.Insert(It.IsAny<UserEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(CreateUserEntity(orgId)))
            .ReturnsAsync(Result.FromError<UserEntity>(new UpdateOperationFailedError()));

        var result = await _sut.InviteUsers(orgId, users, CancellationToken.None);

        Assert.True(result.HasError);
        Assert.IsType<UnexpectedError>(result.Error);
        _userRepoMock.Verify(r => r.Insert(It.IsAny<UserEntity>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task UpdateUser_UpdateFailsAfterSuccessfulPrecheck_ReturnsUserNotFoundError()
    {
        var orgId = Guid.NewGuid();
        var id = Guid.NewGuid();
        SetupOrgExists(orgId);
        _userRepoMock.Setup(r => r.GetById(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(CreateUserEntity(orgId, id)));
        _userRepoMock.Setup(r => r.Update(id, It.IsAny<Action<EntityUpdates<UserEntity>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError(new UpdateOperationFailedError()));

        var result = await _sut.UpdateUser(orgId, id, "New Name", UserRole.Admin, CancellationToken.None);

        Assert.True(result.HasError);
        Assert.IsType<UserNotFoundError>(result.Error);
    }

    [Fact]
    public async Task SoftDeleteUser_RepoReturnsOtherError_ReturnsUnexpectedError()
    {
        var orgId = Guid.NewGuid();
        var id = Guid.NewGuid();
        SetupOrgExists(orgId);
        _userRepoMock.Setup(r => r.GetById(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(CreateUserEntity(orgId, id)));
        _userRepoMock.Setup(r => r.SoftDelete(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError(new EntityNotFoundError<UserEntity>()));

        var result = await _sut.SoftDeleteUser(orgId, id, CancellationToken.None);

        Assert.True(result.HasError);
        Assert.IsType<UnexpectedError>(result.Error);
    }
}
