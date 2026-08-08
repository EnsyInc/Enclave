using EnsyInc.Enclave.Core.Models;

namespace EnsyInc.Enclave.Api.Models.Mappers;

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
