using System.Diagnostics.CodeAnalysis;

using EnsyInc.Enclave.Core.Models;

namespace EnsyInc.Enclave.Api.Models.Mappers;

[SuppressMessage("Naming", "CA1708:Identifiers should differ by more than case", Justification = "The compiler emits a member literally named 'extension' for each C# extension block; there's no way to rename a compiler-synthesized symbol.")]
internal static class LicenseRequestMapper
{
    extension(LicenseRequest coreModel)
    {
        public GetLicenseRequestResponse ToPublicModel()
            => new(
                Id: coreModel.Id,
                OrgId: coreModel.OrgId,
                ProductId: coreModel.ProductId,
                UserId: coreModel.UserId,
                ExistingLicenseId: coreModel.ExistingLicenseId,
                RequestNotes: coreModel.RequestNotes,
                Status: coreModel.Status,
                RejectionReason: coreModel.RejectionReason,
                CreatedAt: coreModel.CreatedAt,
                UpdatedAt: coreModel.UpdatedAt);
    }
}
