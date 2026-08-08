using System.Linq.Expressions;

using EnsyInc.Enclave.Core.Errors;
using EnsyInc.Enclave.Core.Models;
using EnsyInc.Enclave.DataAccess.Abstractions;
using EnsyInc.Enclave.DataAccess.Models;
using EnsyInc.Enclave.Services.Implementations;

using EnsyNet.Core.Results;
using EnsyNet.DataAccess.Abstractions.Errors;
using EnsyNet.DataAccess.Abstractions.Models;

using Moq;

namespace EnsyInc.Enclave.UnitTests.Services;

// Only covers scenarios ServiceTests (black-box, against a real running instance) can't reach:
// repo/DB failures that can't be provoked through the API, and the sequencing case where the
// License write inside Approve succeeds but the follow-up LicenseRequest status update fails
// (can't be reliably forced via real HTTP + a real DB). LicenseRequestNotPendingError,
// LicenseRequestStartDateRequiredError, and LicenseRequestInvalidDateRangeError are all reachable
// via real HTTP calls, so those live in ServiceTests instead.
public sealed class LicenseRequestsServiceTests
{
    private readonly Mock<ILicenseRequestRepo> _licenseRequestRepoMock = new();
    private readonly Mock<ILicenseRepo> _licenseRepoMock = new();
    private readonly LicenseRequestsService _sut;

    public LicenseRequestsServiceTests()
    {
        _sut = new LicenseRequestsService(_licenseRequestRepoMock.Object, _licenseRepoMock.Object);
    }

    private static LicenseRequestEntity CreateRequestEntity(
        Guid? id = null,
        Guid? existingLicenseId = null,
        LicenseRequestStatus status = LicenseRequestStatus.Pending)
        => new()
        {
            Id = id ?? Guid.NewGuid(),
            OrgId = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            ExistingLicenseId = existingLicenseId,
            Status = status,
        };

    private static LicenseEntity CreateLicenseEntity(Guid? id = null)
        => new()
        {
            Id = id ?? Guid.NewGuid(),
            OrgId = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            Start = DateTime.UtcNow.AddDays(-1),
            End = DateTime.UtcNow.AddYears(1),
            Status = LicenseStatus.Active,
        };

    [Fact]
    public async Task ListLicenseRequests_RepoFails_ReturnsUnexpectedError()
    {
        _licenseRequestRepoMock.Setup(r => r.GetManyByExpression(It.IsAny<Expression<Func<LicenseRequestEntity, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError<IEnumerable<LicenseRequestEntity>>(new EntityNotFoundError<LicenseRequestEntity>()));

        var result = await _sut.ListLicenseRequests(null, null, null, CancellationToken.None);

        Assert.True(result.HasError);
        Assert.IsType<UnexpectedError>(result.Error);
    }

    [Fact]
    public async Task GetLicenseRequest_RepoReturnsOtherError_ReturnsUnexpectedError()
    {
        var id = Guid.NewGuid();
        _licenseRequestRepoMock.Setup(r => r.GetById(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError<LicenseRequestEntity>(new UpdateOperationFailedError()));

        var result = await _sut.GetLicenseRequest(id, CancellationToken.None);

        Assert.True(result.HasError);
        Assert.IsType<UnexpectedError>(result.Error);
    }

    [Fact]
    public async Task ApproveLicenseRequest_RepoReturnsOtherError_ReturnsUnexpectedError()
    {
        var id = Guid.NewGuid();
        _licenseRequestRepoMock.Setup(r => r.GetById(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError<LicenseRequestEntity>(new UpdateOperationFailedError()));

        var result = await _sut.ApproveLicenseRequest(id, DateTime.UtcNow, DateTime.UtcNow.AddYears(1), CancellationToken.None);

        Assert.True(result.HasError);
        Assert.IsType<UnexpectedError>(result.Error);
    }

    [Fact]
    public async Task ApproveLicenseRequest_NewRequest_InsertReturnsOtherError_ReturnsUnexpectedError()
    {
        var entity = CreateRequestEntity();
        _licenseRequestRepoMock.Setup(r => r.GetById(entity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(entity));
        _licenseRepoMock.Setup(r => r.Insert(It.IsAny<LicenseEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError<LicenseEntity>(new UpdateOperationFailedError()));

        var result = await _sut.ApproveLicenseRequest(entity.Id, DateTime.UtcNow, DateTime.UtcNow.AddYears(1), CancellationToken.None);

        Assert.True(result.HasError);
        Assert.IsType<UnexpectedError>(result.Error);
        _licenseRequestRepoMock.Verify(r => r.Update(It.IsAny<Guid>(), It.IsAny<Action<EntityUpdates<LicenseRequestEntity>>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ApproveLicenseRequest_NewRequest_UniqueConstraintViolation_ReturnsLicenseAlreadyExistsErrorWithExistingId()
    {
        var entity = CreateRequestEntity();
        var conflictingLicense = CreateLicenseEntity();
        _licenseRequestRepoMock.Setup(r => r.GetById(entity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(entity));
        _licenseRepoMock.Setup(r => r.Insert(It.IsAny<LicenseEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError<LicenseEntity>(new UniqueConstraintViolationError(new InvalidOperationException())));
        _licenseRepoMock.Setup(r => r.GetByExpression(It.IsAny<Expression<Func<LicenseEntity, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(conflictingLicense));

        var result = await _sut.ApproveLicenseRequest(entity.Id, DateTime.UtcNow, DateTime.UtcNow.AddYears(1), CancellationToken.None);

        Assert.True(result.HasError);
        var error = Assert.IsType<LicenseAlreadyExistsError>(result.Error);
        Assert.Equal(conflictingLicense.Id, error.ExistingLicenseId);
    }

    [Fact]
    public async Task ApproveLicenseRequest_Renewal_ExistingLicenseRepoReturnsOtherError_ReturnsUnexpectedError()
    {
        var existingLicenseId = Guid.NewGuid();
        var entity = CreateRequestEntity(existingLicenseId: existingLicenseId);
        _licenseRequestRepoMock.Setup(r => r.GetById(entity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(entity));
        _licenseRepoMock.Setup(r => r.GetById(existingLicenseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError<LicenseEntity>(new UpdateOperationFailedError()));

        var result = await _sut.ApproveLicenseRequest(entity.Id, null, DateTime.UtcNow.AddYears(1), CancellationToken.None);

        Assert.True(result.HasError);
        Assert.IsType<UnexpectedError>(result.Error);
    }

    [Fact]
    public async Task ApproveLicenseRequest_Renewal_LicenseUpdateFailsAfterSuccessfulFetch_ReturnsLicenseNotFoundError()
    {
        var existingLicense = CreateLicenseEntity();
        var entity = CreateRequestEntity(existingLicenseId: existingLicense.Id);
        _licenseRequestRepoMock.Setup(r => r.GetById(entity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(entity));
        _licenseRepoMock.Setup(r => r.GetById(existingLicense.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(existingLicense));
        _licenseRepoMock.Setup(r => r.Update(existingLicense.Id, It.IsAny<Action<EntityUpdates<LicenseEntity>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError(new UpdateOperationFailedError()));

        var result = await _sut.ApproveLicenseRequest(entity.Id, null, existingLicense.End.AddYears(1), CancellationToken.None);

        Assert.True(result.HasError);
        Assert.IsType<LicenseNotFoundError>(result.Error);
    }

    [Fact]
    public async Task ApproveLicenseRequest_LicenseWriteSucceedsButStatusUpdateFails_ReturnsLicenseRequestNotFoundError()
    {
        var entity = CreateRequestEntity();
        _licenseRequestRepoMock.Setup(r => r.GetById(entity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(entity));
        _licenseRepoMock.Setup(r => r.Insert(It.IsAny<LicenseEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(CreateLicenseEntity()));
        _licenseRequestRepoMock.Setup(r => r.Update(entity.Id, It.IsAny<Action<EntityUpdates<LicenseRequestEntity>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError(new UpdateOperationFailedError()));

        var result = await _sut.ApproveLicenseRequest(entity.Id, DateTime.UtcNow, DateTime.UtcNow.AddYears(1), CancellationToken.None);

        Assert.True(result.HasError);
        Assert.IsType<LicenseRequestNotFoundError>(result.Error);
    }

    [Fact]
    public async Task RejectLicenseRequest_RepoReturnsOtherError_ReturnsUnexpectedError()
    {
        var id = Guid.NewGuid();
        _licenseRequestRepoMock.Setup(r => r.GetById(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError<LicenseRequestEntity>(new UpdateOperationFailedError()));

        var result = await _sut.RejectLicenseRequest(id, "not a fit", CancellationToken.None);

        Assert.True(result.HasError);
        Assert.IsType<UnexpectedError>(result.Error);
    }

    [Fact]
    public async Task RejectLicenseRequest_UpdateFailsAfterSuccessfulPrecheck_ReturnsLicenseRequestNotFoundError()
    {
        var entity = CreateRequestEntity();
        _licenseRequestRepoMock.Setup(r => r.GetById(entity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(entity));
        _licenseRequestRepoMock.Setup(r => r.Update(entity.Id, It.IsAny<Action<EntityUpdates<LicenseRequestEntity>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError(new UpdateOperationFailedError()));

        var result = await _sut.RejectLicenseRequest(entity.Id, null, CancellationToken.None);

        Assert.True(result.HasError);
        Assert.IsType<LicenseRequestNotFoundError>(result.Error);
    }
}
