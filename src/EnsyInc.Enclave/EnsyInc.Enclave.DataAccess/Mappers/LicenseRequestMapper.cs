using EnsyInc.Enclave.Core.Models;
using EnsyInc.Enclave.DataAccess.Models;

namespace EnsyInc.Enclave.DataAccess.Mappers;

public static class LicenseRequestMapper
{
    extension(LicenseRequestEntity entity)
    {
        public LicenseRequest ToCoreModel()
            => new()
            {
                Id = entity.Id,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                DeletedAt = entity.DeletedAt,
                OrgId = entity.OrgId,
                ProductId = entity.ProductId,
                UserId = entity.UserId,
                ExistingLicenseId = entity.ExistingLicenseId,
                RequestNotes = entity.RequestNotes,
                Status = entity.Status,
                RejectionReason = entity.RejectionReason,
            };
    }

    extension(LicenseRequest coreModel)
    {
        public LicenseRequestEntity ToEntityModel()
            => new()
            {
                Id = coreModel.Id,
                CreatedAt = coreModel.CreatedAt,
                UpdatedAt = coreModel.UpdatedAt,
                DeletedAt = coreModel.DeletedAt,
                OrgId = coreModel.OrgId,
                ProductId = coreModel.ProductId,
                UserId = coreModel.UserId,
                ExistingLicenseId = coreModel.ExistingLicenseId,
                RequestNotes = coreModel.RequestNotes,
                Status = coreModel.Status,
                RejectionReason = coreModel.RejectionReason,
            };
    }
}
