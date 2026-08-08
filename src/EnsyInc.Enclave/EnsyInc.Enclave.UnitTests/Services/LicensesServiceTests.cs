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
// repo/DB failures that can't be provoked through the API (including the org/product existence
// pre-checks, and the secondary lookup used to build LicenseAlreadyExistsError). SuspendLicense
// and RevokeLicense share the exact same fetch-then-update code path, so it's exercised fully
// once (via Suspend) rather than duplicated for Revoke.
public sealed class LicensesServiceTests
{
    private readonly Mock<ILicenseRepo> _licenseRepoMock = new();
    private readonly Mock<IOrgRepo> _orgRepoMock = new();
    private readonly Mock<IProductRepo> _productRepoMock = new();
    private readonly LicensesService _sut;

    public LicensesServiceTests()
    {
        _sut = new LicensesService(_licenseRepoMock.Object, _orgRepoMock.Object, _productRepoMock.Object);
    }

    private static OrgEntity CreateOrgEntity(Guid? id = null)
        => new()
        {
            Id = id ?? Guid.NewGuid(),
            Name = "Acme",
            Status = OrgStatus.Active,
        };

    private static ProductEntity CreateProductEntity(Guid? id = null)
        => new()
        {
            Id = id ?? Guid.NewGuid(),
            Name = "Widget",
            Status = ProductStatus.Active,
        };

    private static LicenseEntity CreateLicenseEntity(Guid? id = null, Guid? orgId = null, Guid? productId = null, LicenseStatus status = LicenseStatus.Active)
        => new()
        {
            Id = id ?? Guid.NewGuid(),
            OrgId = orgId ?? Guid.NewGuid(),
            ProductId = productId ?? Guid.NewGuid(),
            Start = DateTime.UtcNow.AddDays(-1),
            End = DateTime.UtcNow.AddYears(1),
            Status = status,
        };

    private void SetupOrgExists(Guid orgId)
        => _orgRepoMock.Setup(r => r.GetById(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(CreateOrgEntity(orgId)));

    private void SetupProductExists(Guid productId)
        => _productRepoMock.Setup(r => r.GetById(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(CreateProductEntity(productId)));

    [Fact]
    public async Task ListLicenses_RepoFails_ReturnsUnexpectedError()
    {
        _licenseRepoMock.Setup(r => r.GetManyByExpression(It.IsAny<Expression<Func<LicenseEntity, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError<IEnumerable<LicenseEntity>>(new EntityNotFoundError<LicenseEntity>()));

        var result = await _sut.ListLicenses(null, null, null, CancellationToken.None);

        Assert.True(result.HasError);
        Assert.IsType<UnexpectedError>(result.Error);
    }

    [Fact]
    public async Task GetLicense_RepoReturnsOtherError_ReturnsUnexpectedError()
    {
        var id = Guid.NewGuid();
        _licenseRepoMock.Setup(r => r.GetById(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError<LicenseEntity>(new UpdateOperationFailedError()));

        var result = await _sut.GetLicense(id, CancellationToken.None);

        Assert.True(result.HasError);
        Assert.IsType<UnexpectedError>(result.Error);
    }

    [Fact]
    public async Task GrantLicense_OrgRepoFails_ReturnsUnexpectedError()
    {
        var orgId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        _orgRepoMock.Setup(r => r.GetById(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError<OrgEntity>(new UpdateOperationFailedError()));

        var result = await _sut.GrantLicense(orgId, productId, DateTime.UtcNow, DateTime.UtcNow.AddYears(1), CancellationToken.None);

        Assert.True(result.HasError);
        Assert.IsType<UnexpectedError>(result.Error);
        _productRepoMock.Verify(r => r.GetById(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GrantLicense_ProductRepoFails_ReturnsUnexpectedError()
    {
        var orgId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        SetupOrgExists(orgId);
        _productRepoMock.Setup(r => r.GetById(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError<ProductEntity>(new UpdateOperationFailedError()));

        var result = await _sut.GrantLicense(orgId, productId, DateTime.UtcNow, DateTime.UtcNow.AddYears(1), CancellationToken.None);

        Assert.True(result.HasError);
        Assert.IsType<UnexpectedError>(result.Error);
    }

    [Fact]
    public async Task GrantLicense_InsertReturnsOtherError_ReturnsUnexpectedError()
    {
        var orgId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        SetupOrgExists(orgId);
        SetupProductExists(productId);
        _licenseRepoMock.Setup(r => r.Insert(It.IsAny<LicenseEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError<LicenseEntity>(new UpdateOperationFailedError()));

        var result = await _sut.GrantLicense(orgId, productId, DateTime.UtcNow, DateTime.UtcNow.AddYears(1), CancellationToken.None);

        Assert.True(result.HasError);
        Assert.IsType<UnexpectedError>(result.Error);
    }

    [Fact]
    public async Task GrantLicense_UniqueConstraintViolation_LookupOfExistingLicenseFails_ReturnsUnexpectedError()
    {
        var orgId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        SetupOrgExists(orgId);
        SetupProductExists(productId);
        _licenseRepoMock.Setup(r => r.Insert(It.IsAny<LicenseEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError<LicenseEntity>(new UniqueConstraintViolationError(new InvalidOperationException())));
        _licenseRepoMock.Setup(r => r.GetByExpression(It.IsAny<Expression<Func<LicenseEntity, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError<LicenseEntity>(new EntityNotFoundError<LicenseEntity>()));

        var result = await _sut.GrantLicense(orgId, productId, DateTime.UtcNow, DateTime.UtcNow.AddYears(1), CancellationToken.None);

        Assert.True(result.HasError);
        Assert.IsType<UnexpectedError>(result.Error);
    }

    [Fact]
    public async Task GrantLicense_UniqueConstraintViolation_ReturnsLicenseAlreadyExistsErrorWithExistingId()
    {
        var orgId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var existingLicense = CreateLicenseEntity(orgId: orgId, productId: productId);
        SetupOrgExists(orgId);
        SetupProductExists(productId);
        _licenseRepoMock.Setup(r => r.Insert(It.IsAny<LicenseEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError<LicenseEntity>(new UniqueConstraintViolationError(new InvalidOperationException())));
        _licenseRepoMock.Setup(r => r.GetByExpression(It.IsAny<Expression<Func<LicenseEntity, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(existingLicense));

        var result = await _sut.GrantLicense(orgId, productId, DateTime.UtcNow, DateTime.UtcNow.AddYears(1), CancellationToken.None);

        Assert.True(result.HasError);
        var error = Assert.IsType<LicenseAlreadyExistsError>(result.Error);
        Assert.Equal(existingLicense.Id, error.ExistingLicenseId);
    }

    [Fact]
    public async Task UpdateLicenseDates_UpdateFailsAfterSuccessfulPrecheck_ReturnsLicenseNotFoundError()
    {
        var entity = CreateLicenseEntity();
        _licenseRepoMock.Setup(r => r.GetById(entity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(entity));
        _licenseRepoMock.Setup(r => r.Update(entity.Id, It.IsAny<Action<EntityUpdates<LicenseEntity>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError(new UpdateOperationFailedError()));

        var result = await _sut.UpdateLicenseDates(entity.Id, DateTime.UtcNow, DateTime.UtcNow.AddYears(1), CancellationToken.None);

        Assert.True(result.HasError);
        Assert.IsType<LicenseNotFoundError>(result.Error);
    }

    [Fact]
    public async Task SuspendLicense_UpdateFailsAfterSuccessfulPrecheck_ReturnsLicenseNotFoundError()
    {
        var entity = CreateLicenseEntity();
        _licenseRepoMock.Setup(r => r.GetById(entity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(entity));
        _licenseRepoMock.Setup(r => r.Update(entity.Id, It.IsAny<Action<EntityUpdates<LicenseEntity>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError(new UpdateOperationFailedError()));

        var result = await _sut.SuspendLicense(entity.Id, CancellationToken.None);

        Assert.True(result.HasError);
        Assert.IsType<LicenseNotFoundError>(result.Error);
    }

    [Fact]
    public async Task SoftDeleteLicense_RepoReturnsOtherError_ReturnsUnexpectedError()
    {
        var id = Guid.NewGuid();
        _licenseRepoMock.Setup(r => r.SoftDelete(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError(new EntityNotFoundError<LicenseEntity>()));

        var result = await _sut.SoftDeleteLicense(id, CancellationToken.None);

        Assert.True(result.HasError);
        Assert.IsType<UnexpectedError>(result.Error);
    }
}
