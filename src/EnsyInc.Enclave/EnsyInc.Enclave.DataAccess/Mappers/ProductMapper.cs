using EnsyInc.Enclave.Core.Models;
using EnsyInc.Enclave.DataAccess.Models;

namespace EnsyInc.Enclave.DataAccess.Mappers;

public static class ProductMapper
{
    extension(ProductEntity entity)
    {
        public Product ToCoreModel()
            => new()
            {
                Id = entity.Id,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                DeletedAt = entity.DeletedAt,
                Name = entity.Name,
                Description = entity.Description,
                Status = entity.Status,
            };
    }

    extension(Product coreModel)
    {
        public ProductEntity ToEntityModel()
            => new()
            {
                Id = coreModel.Id,
                CreatedAt = coreModel.CreatedAt,
                UpdatedAt = coreModel.UpdatedAt,
                DeletedAt = coreModel.DeletedAt,
                Name = coreModel.Name,
                Description = coreModel.Description,
                Status = coreModel.Status,
            };
    }
}
