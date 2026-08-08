using EnsyInc.Enclave.Core.Models;
using EnsyInc.Enclave.DataAccess.Models;

namespace EnsyInc.Enclave.DataAccess.Mappers;

public static class UserMapper
{
    extension(UserEntity entity)
    {
        public User ToCoreModel()
            => new()
            {
                Id = entity.Id,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                DeletedAt = entity.DeletedAt,
                Name = entity.Name,
                Email = entity.Email,
                OrgId = entity.OrgId,
                Status = entity.Status,
                Role = entity.Role,
            };
    }

    extension(User coreModel)
    {
        public UserEntity ToEntityModel()
            => new()
            {
                Id = coreModel.Id,
                CreatedAt = coreModel.CreatedAt,
                UpdatedAt = coreModel.UpdatedAt,
                DeletedAt = coreModel.DeletedAt,
                Name = coreModel.Name,
                Email = coreModel.Email,
                OrgId = coreModel.OrgId,
                Status = coreModel.Status,
                Role = coreModel.Role,
            };
    }
}
