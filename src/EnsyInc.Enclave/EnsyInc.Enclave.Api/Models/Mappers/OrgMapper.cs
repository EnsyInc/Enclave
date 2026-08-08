using System.Diagnostics.CodeAnalysis;

using EnsyInc.Enclave.Core.Models;

namespace EnsyInc.Enclave.Api.Models.Mappers;

[SuppressMessage("Naming", "CA1708:Identifiers should differ by more than case", Justification = "The compiler emits a member literally named 'extension' for each C# extension block; there's no way to rename a compiler-synthesized symbol.")]
internal static class OrgMapper
{
    extension(Org coreModel)
    {
        public GetOrgResponse ToPublicModel()
            => new(
                Id: coreModel.Id,
                Name: coreModel.Name,
                Status: coreModel.Status,
                PrimaryUserId: coreModel.PrimaryUserId,
                CreatedAt: coreModel.CreatedAt,
                UpdatedAt: coreModel.UpdatedAt);
    }

    extension(CreateOrgRequest publicModel)
    {
        public Org ToCoreModel()
            => new()
            {
                Name = publicModel.Name,
                Status = OrgStatus.Active,
            };
    }

    extension(UpdateOrgRequest publicModel)
    {
        public Org ToCoreModel(Guid id)
            => new()
            {
                Id = id,
                Name = publicModel.Name,
                Status = OrgStatus.Active,
            };
    }
}
