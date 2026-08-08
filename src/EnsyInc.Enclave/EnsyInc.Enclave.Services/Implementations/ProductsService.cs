using EnsyInc.Enclave.Core.Errors;
using EnsyInc.Enclave.Core.Models;
using EnsyInc.Enclave.DataAccess.Abstractions;
using EnsyInc.Enclave.DataAccess.Mappers;
using EnsyInc.Enclave.DataAccess.Models;
using EnsyInc.Enclave.Services.Abstractions;

using EnsyNet.Core.Results;
using EnsyNet.DataAccess.Abstractions.Errors;

namespace EnsyInc.Enclave.Services.Implementations;

internal sealed class ProductsService(IProductRepo productRepo) : IProductsService
{
    public async Task<Result<IEnumerable<Product>>> ListProduct(CancellationToken ct)
    {
        var result = await productRepo.GetAll(ct);
        return result.HasError
            ? Result.FromError<IEnumerable<Product>>(new UnexpectedError())
            : Result.Ok(result.Data.Select(e => e.ToCoreModel()));
    }

    public async Task<Result<IEnumerable<Product>>> ListProductByName(string name, CancellationToken ct)
    {
        // string.Contains(string, StringComparison) can't be translated by EF Core to SQL Server;
        // plain Contains() translates to LIKE and is already case-insensitive under SQL Server's
        // default (case-insensitive) collation.
        var result = await productRepo.GetManyByExpression(p => p.Name.Contains(name), ct);
        return result.HasError
            ? Result.FromError<IEnumerable<Product>>(new UnexpectedError())
            : Result.Ok(result.Data.Select(e => e.ToCoreModel()));
    }

    public async Task<Result<Product?>> GetProduct(Guid id, CancellationToken ct)
    {
        var result = await productRepo.GetById(id, ct);

        if (result.HasError)
        {
            return result.Error switch
            {
                EntityNotFoundError<ProductEntity> => Result.FromError<Product?>(new ProductNotFoundError()),
                _ => Result.FromError<Product?>(new UnexpectedError()),
            };
        }

        return Result.Ok<Product?>(result.Data.ToCoreModel());
    }

    public async Task<Result<Product>> CreateProduct(Product product)
    {
        var result = await productRepo.Insert(product.ToEntityModel(), CancellationToken.None);

        if (result.HasError)
        {
            return result.Error switch
            {
                EntityNotFoundError<ProductEntity> => Result.FromError<Product>(new ProductNotFoundError()),
                _ => Result.FromError<Product>(new UnexpectedError()),
            };
        }

        return Result.Ok(result.Data.ToCoreModel());
    }

    public async Task<Result<Product>> UpdateProduct(Product product, CancellationToken ct)
    {
        var updateResult = await productRepo.Update(product.Id, updates =>
        {
            updates.AddUpdate(p => p.Name, _ => product.Name);
            updates.AddUpdate(p => p.Description, _ => product.Description);
            updates.AddUpdate(p => p.Status, _ => product.Status);
        }, ct);

        if (updateResult.HasError)
        {
            return updateResult.Error switch
            {
                UpdateOperationFailedError => Result.FromError<Product>(new ProductNotFoundError()),
                _ => Result.FromError<Product>(new UnexpectedError()),
            };
        }

        var getResult = await productRepo.GetById(product.Id, ct);

        if (getResult.HasError)
        {
            return getResult.Error switch
            {
                EntityNotFoundError<ProductEntity> => Result.FromError<Product>(new ProductNotFoundError()),
                _ => Result.FromError<Product>(new UnexpectedError()),
            };
        }

        return Result.Ok(getResult.Data.ToCoreModel());
    }

    public async Task<Result<Product>> RetireProduct(Guid id, CancellationToken ct)
    {
        var updateResult = await productRepo.Update(id, updates => updates.AddUpdate(p => p.Status, _ => ProductStatus.Retired), ct);

        if (updateResult.HasError)
        {
            return updateResult.Error switch
            {
                UpdateOperationFailedError => Result.FromError<Product>(new ProductNotFoundError()),
                _ => Result.FromError<Product>(new UnexpectedError()),
            };
        }

        var getResult = await productRepo.GetById(id, ct);

        if (getResult.HasError)
        {
            return getResult.Error switch
            {
                EntityNotFoundError<ProductEntity> => Result.FromError<Product>(new ProductNotFoundError()),
                _ => Result.FromError<Product>(new UnexpectedError()),
            };
        }

        return Result.Ok(getResult.Data.ToCoreModel());
    }

    public async Task<Result<bool>> SoftDeleteProduct(Guid id, CancellationToken ct)
    {
        var result = await productRepo.SoftDelete(id, ct);

        if (result.HasError)
        {
            return result.Error switch
            {
                DeleteOperationFailedError => Result.Ok(true),
                _ => Result.FromError<bool>(new UnexpectedError()),
            };
        }

        return Result.Ok(true);
    }

    public async Task<Result<bool>> HardDeleteProduct(Guid id, CancellationToken ct)
    {
        var result = await productRepo.HardDelete(id, ct);

        if (result.HasError)
        {
            return result.Error switch
            {
                DeleteOperationFailedError => Result.Ok(true),
                _ => Result.FromError<bool>(new UnexpectedError()),
            };
        }

        return Result.Ok(true);
    }
}
