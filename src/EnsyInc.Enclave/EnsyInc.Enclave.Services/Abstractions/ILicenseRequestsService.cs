using EnsyInc.Enclave.Core.Models;

using EnsyNet.Core.Results;

namespace EnsyInc.Enclave.Services.Abstractions;

public interface ILicenseRequestsService
{
    public Task<Result<IEnumerable<LicenseRequest>>> ListLicenseRequests(Guid? orgId, Guid? productId, LicenseRequestStatus? status, CancellationToken ct);
    public Task<Result<LicenseRequest?>> GetLicenseRequest(Guid id, CancellationToken ct);

    public Task<Result<LicenseRequest>> ApproveLicenseRequest(Guid id, DateTime? start, DateTime end, CancellationToken ct);
    public Task<Result<LicenseRequest>> RejectLicenseRequest(Guid id, string? reason, CancellationToken ct);
}
