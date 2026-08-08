using EnsyInc.Enclave.DataAccess.Abstractions;
using EnsyInc.Enclave.DataAccess.Models;

using EnsyNet.DataAccess.EntityFramework;

using Microsoft.Extensions.Logging;

namespace EnsyInc.Enclave.DataAccess.EF.Implementations;

internal sealed class ProductRepo : BaseRepository<ProductEntity>, IProductRepo
{
    public ProductRepo(EnclaveDbContext dbContext, ILogger<ProductRepo> logger) : base(dbContext, dbContext.Products, logger) { }
}
