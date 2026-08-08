using EnsyInc.Enclave.Core.Models;
using EnsyInc.Enclave.DataAccess.Models;

namespace EnsyInc.Enclave.DataAccess.Mappers;

public static class LicenseMapper
{
    extension(LicenseEntity entity)
    {
        public License ToCoreModel()
            => new()
            {
                Id = entity.Id,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                DeletedAt = entity.DeletedAt,
                OrgId = entity.OrgId,
                ProductId = entity.ProductId,
                Start = entity.Start,
                End = entity.End,
                Status = entity.Status,
            };
    }

    extension(License coreModel)
    {
        public LicenseEntity ToEntityModel()
            => new()
            {
                Id = coreModel.Id,
                CreatedAt = coreModel.CreatedAt,
                UpdatedAt = coreModel.UpdatedAt,
                DeletedAt = coreModel.DeletedAt,
                OrgId = coreModel.OrgId,
                ProductId = coreModel.ProductId,
                Start = coreModel.Start,
                End = coreModel.End,
                Status = coreModel.Status,
            };
    }
}
