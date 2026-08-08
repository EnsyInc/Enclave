using System.Diagnostics.CodeAnalysis;

using EnsyInc.Enclave.Core.Models;

namespace EnsyInc.Enclave.Api.Models.Mappers;

[SuppressMessage("Naming", "CA1708:Identifiers should differ by more than case", Justification = "The compiler emits a member literally named 'extension' for each C# extension block; there's no way to rename a compiler-synthesized symbol.")]
internal static class ProductMapper
{
    extension(Product coreModel)
    {
        public GetProductResponse ToPublicModel() 
            => new(
                Id: coreModel.Id,
                Name: coreModel.Name,
                Description: coreModel.Description,
                Status: coreModel.Status,
                CreatedAt: coreModel.CreatedAt,
                UpdatedAt: coreModel.UpdatedAt);
    }

    extension(CreateProductRequest publicModel)
    {
        public Product ToCoreModel()
            => new()
            {
                Name = publicModel.Name,
                Description = publicModel.Description,
                Status = publicModel.Status,
            };
    }

    extension(UpdateProductRequest publicModel)
    {
        public Product ToCoreModel(Guid id)
            => new()
            {
                Id = id,
                Name = publicModel.Name,
                Description = publicModel.Description,
                Status = publicModel.Status,
            };
    }
}
