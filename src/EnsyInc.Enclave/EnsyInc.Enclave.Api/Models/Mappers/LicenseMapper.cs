using System.Diagnostics.CodeAnalysis;

using EnsyInc.Enclave.Core.Models;

namespace EnsyInc.Enclave.Api.Models.Mappers;

[SuppressMessage("Naming", "CA1708:Identifiers should differ by more than case", Justification = "The compiler emits a member literally named 'extension' for each C# extension block; there's no way to rename a compiler-synthesized symbol.")]
internal static class LicenseMapper
{
    extension(License coreModel)
    {
        public GetLicenseResponse ToPublicModel()
            => new(
                Id: coreModel.Id,
                OrgId: coreModel.OrgId,
                ProductId: coreModel.ProductId,
                Status: coreModel.Status,
                Start: coreModel.Start,
                End: coreModel.End,
                CreatedAt: coreModel.CreatedAt,
                UpdatedAt: coreModel.UpdatedAt);
    }
}
