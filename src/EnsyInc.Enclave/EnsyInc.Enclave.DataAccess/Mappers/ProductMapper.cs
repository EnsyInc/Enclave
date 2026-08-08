using System.Diagnostics.CodeAnalysis;

using EnsyInc.Enclave.Core.Models;
using EnsyInc.Enclave.DataAccess.Models;

namespace EnsyInc.Enclave.DataAccess.Mappers;

[SuppressMessage("Naming", "CA1708:Identifiers should differ by more than case", Justification = "The compiler emits a member literally named 'extension' for each C# extension block; there's no way to rename a compiler-synthesized symbol.")]
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
