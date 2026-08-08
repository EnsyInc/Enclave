using EnsyInc.Enclave.Core.Models;

using EnsyNet.Core.Results;

namespace EnsyInc.Enclave.Services.Abstractions;

public interface IProductsService
{
    public Task<Result<IEnumerable<Product>>> ListProduct(CancellationToken ct);
    public Task<Result<IEnumerable<Product>>> ListProductByName(string name, CancellationToken ct);
    public Task<Result<Product?>> GetProduct(Guid id, CancellationToken ct);

    public Task<Result<Product>> CreateProduct(Product product);

    public Task<Result<Product>> UpdateProduct(Product product, CancellationToken ct);
    public Task<Result<Product>> RetireProduct(Guid id, CancellationToken ct);

    public Task<Result<bool>> SoftDeleteProduct(Guid id, CancellationToken ct);
    public Task<Result<bool>> HardDeleteProduct(Guid id, CancellationToken ct);
}
