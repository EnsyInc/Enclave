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
// repo/DB failures that can't be provoked through the API, internal call-sequencing assertions
// only visible via mock Verify, and HardDelete (no HTTP route exists).
public sealed class ProductsServiceTests
{
    private readonly Mock<IProductRepo> _productRepoMock = new();
    private readonly ProductsService _sut;

    public ProductsServiceTests()
    {
        _sut = new ProductsService(_productRepoMock.Object);
    }

    private static ProductEntity CreateEntity(Guid? id = null, string name = "Widget", ProductStatus status = ProductStatus.Active)
        => new()
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Description = "A widget",
            Status = status,
        };

    [Fact]
    public async Task ListProduct_RepoFails_ReturnsUnexpectedError()
    {
        _productRepoMock.Setup(r => r.GetAll(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError<IEnumerable<ProductEntity>>(new EntityNotFoundError<ProductEntity>()));

        var result = await _sut.ListProduct(CancellationToken.None);

        Assert.True(result.HasError);
        Assert.IsType<UnexpectedError>(result.Error);
    }

    [Fact]
    public async Task ListProductByName_RepoFails_ReturnsUnexpectedError()
    {
        _productRepoMock.Setup(r => r.GetManyByExpression(It.IsAny<Expression<Func<ProductEntity, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError<IEnumerable<ProductEntity>>(new EntityNotFoundError<ProductEntity>()));

        var result = await _sut.ListProductByName("Gadget", CancellationToken.None);

        Assert.True(result.HasError);
        Assert.IsType<UnexpectedError>(result.Error);
    }

    [Fact]
    public async Task GetProduct_RepoReturnsOtherError_ReturnsUnexpectedError()
    {
        var id = Guid.NewGuid();
        _productRepoMock.Setup(r => r.GetById(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError<ProductEntity>(new UpdateOperationFailedError()));

        var result = await _sut.GetProduct(id, CancellationToken.None);

        Assert.True(result.HasError);
        Assert.IsType<UnexpectedError>(result.Error);
    }

    [Fact]
    public async Task CreateProduct_RepoReturnsNotFound_ReturnsProductNotFoundError()
    {
        var product = CreateEntity().ToCoreModel();
        _productRepoMock.Setup(r => r.Insert(It.IsAny<ProductEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError<ProductEntity>(new EntityNotFoundError<ProductEntity>()));

        var result = await _sut.CreateProduct(product);

        Assert.True(result.HasError);
        Assert.IsType<ProductNotFoundError>(result.Error);
    }

    [Fact]
    public async Task CreateProduct_RepoReturnsOtherError_ReturnsUnexpectedError()
    {
        var product = CreateEntity().ToCoreModel();
        _productRepoMock.Setup(r => r.Insert(It.IsAny<ProductEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError<ProductEntity>(new UpdateOperationFailedError()));

        var result = await _sut.CreateProduct(product);

        Assert.True(result.HasError);
        Assert.IsType<UnexpectedError>(result.Error);
    }

    [Fact]
    public async Task UpdateProduct_UpdateFailsAfterSuccessfulPrecheck_ReturnsProductNotFoundError()
    {
        var entity = CreateEntity();
        var product = entity.ToCoreModel();
        _productRepoMock.Setup(r => r.GetById(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(entity));
        _productRepoMock.Setup(r => r.Update(product.Id, It.IsAny<Action<EntityUpdates<ProductEntity>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError(new UpdateOperationFailedError()));

        var result = await _sut.UpdateProduct(product, CancellationToken.None);

        Assert.True(result.HasError);
        Assert.IsType<ProductNotFoundError>(result.Error);
    }

    [Fact]
    public async Task RetireProduct_UpdateFailsAfterSuccessfulPrecheck_ReturnsProductNotFoundError()
    {
        var id = Guid.NewGuid();
        _productRepoMock.Setup(r => r.GetById(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(CreateEntity(id)));
        _productRepoMock.Setup(r => r.Update(id, It.IsAny<Action<EntityUpdates<ProductEntity>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError(new UpdateOperationFailedError()));

        var result = await _sut.RetireProduct(id, CancellationToken.None);

        Assert.True(result.HasError);
        Assert.IsType<ProductNotFoundError>(result.Error);
    }

    [Fact]
    public async Task SoftDeleteProduct_RepoReturnsOtherError_ReturnsUnexpectedError()
    {
        var id = Guid.NewGuid();
        _productRepoMock.Setup(r => r.SoftDelete(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError(new EntityNotFoundError<ProductEntity>()));

        var result = await _sut.SoftDeleteProduct(id, CancellationToken.None);

        Assert.True(result.HasError);
        Assert.IsType<UnexpectedError>(result.Error);
    }

    [Fact]
    public async Task HardDeleteProduct_RepoSucceeds_ReturnsTrue()
    {
        var id = Guid.NewGuid();
        _productRepoMock.Setup(r => r.HardDelete(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        var result = await _sut.HardDeleteProduct(id, CancellationToken.None);

        Assert.False(result.HasError);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task HardDeleteProduct_RepoReturnsDeleteOperationFailed_ReturnsTrueIdempotently()
    {
        var id = Guid.NewGuid();
        _productRepoMock.Setup(r => r.HardDelete(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError(new DeleteOperationFailedError()));

        var result = await _sut.HardDeleteProduct(id, CancellationToken.None);

        Assert.False(result.HasError);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task HardDeleteProduct_RepoReturnsOtherError_ReturnsUnexpectedError()
    {
        var id = Guid.NewGuid();
        _productRepoMock.Setup(r => r.HardDelete(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError(new EntityNotFoundError<ProductEntity>()));

        var result = await _sut.HardDeleteProduct(id, CancellationToken.None);

        Assert.True(result.HasError);
        Assert.IsType<UnexpectedError>(result.Error);
    }
}
