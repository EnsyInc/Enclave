using System.Diagnostics.CodeAnalysis;

using EnsyInc.Enclave.Core.Models;
using EnsyInc.Enclave.DataAccess.Models;

namespace EnsyInc.Enclave.DataAccess.Mappers;

[SuppressMessage("Naming", "CA1708:Identifiers should differ by more than case", Justification = "The compiler emits a member literally named 'extension' for each C# extension block; there's no way to rename a compiler-synthesized symbol.")]
public static class OrgMapper
{
    extension(OrgEntity entity)
    {
        public Org ToCoreModel()
            => new()
            {
                Id = entity.Id,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                DeletedAt = entity.DeletedAt,
                Name = entity.Name,
                Status = entity.Status,
                PrimaryUserId = entity.PrimaryUserId,
            };
    }

    extension(Org coreModel)
    {
        public OrgEntity ToEntityModel()
            => new()
            {
                Id = coreModel.Id,
                CreatedAt = coreModel.CreatedAt,
                UpdatedAt = coreModel.UpdatedAt,
                DeletedAt = coreModel.DeletedAt,
                Name = coreModel.Name,
                Status = coreModel.Status,
                PrimaryUserId = coreModel.PrimaryUserId,
            };
    }
}
