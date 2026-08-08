using System.Diagnostics.CodeAnalysis;

using EnsyInc.Enclave.Core.Models;

namespace EnsyInc.Enclave.Api.Models.Mappers;

[SuppressMessage("Naming", "CA1708:Identifiers should differ by more than case", Justification = "The compiler emits a member literally named 'extension' for each C# extension block; there's no way to rename a compiler-synthesized symbol.")]
internal static class UserMapper
{
    extension(User coreModel)
    {
        public GetUserResponse ToPublicModel()
            => new(
                Id: coreModel.Id,
                Name: coreModel.Name,
                Email: coreModel.Email,
                OrgId: coreModel.OrgId,
                Status: coreModel.Status,
                Role: coreModel.Role,
                CreatedAt: coreModel.CreatedAt,
                UpdatedAt: coreModel.UpdatedAt);
    }

    extension(InviteUserRequest publicModel)
    {
        public User ToCoreModel(Guid orgId)
            => new()
            {
                Name = publicModel.Name,
                Email = publicModel.Email,
                OrgId = orgId,
                Status = UserStatus.InviteSent,
                Role = publicModel.Role,
            };
    }

}
