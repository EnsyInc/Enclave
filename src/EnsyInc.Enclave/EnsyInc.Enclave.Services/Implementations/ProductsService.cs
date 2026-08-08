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
        var existing = await GetProduct(product.Id, ct);
        if (existing.HasError)
        {
            return Result.FromError<Product>(existing.Error);
        }

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

        // The update already told us it succeeded, so the new state is known without re-querying:
        // apply the same field changes to the entity fetched above instead of fetching again.
        return Result.Ok(existing.Data with { Name = product.Name, Description = product.Description, Status = product.Status, UpdatedAt = DateTime.UtcNow });
    }

    public async Task<Result<Product>> RetireProduct(Guid id, CancellationToken ct)
    {
        var existing = await GetProduct(id, ct);
        if (existing.HasError)
        {
            return Result.FromError<Product>(existing.Error);
        }

        var updateResult = await productRepo.Update(id, updates => updates.AddUpdate(p => p.Status, _ => ProductStatus.Retired), ct);

        if (updateResult.HasError)
        {
            return updateResult.Error switch
            {
                UpdateOperationFailedError => Result.FromError<Product>(new ProductNotFoundError()),
                _ => Result.FromError<Product>(new UnexpectedError()),
            };
        }

        return Result.Ok(existing.Data with { Status = ProductStatus.Retired, UpdatedAt = DateTime.UtcNow });
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
