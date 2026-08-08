using EnsyInc.Enclave.Core.Models;

using EnsyNet.Core.Results;

namespace EnsyInc.Enclave.Services.Abstractions;

public interface ILicensesService
{
    public Task<Result<IEnumerable<License>>> ListLicenses(Guid? orgId, Guid? productId, LicenseStatus? status, CancellationToken ct);
    public Task<Result<License?>> GetLicense(Guid id, CancellationToken ct);

    public Task<Result<License>> GrantLicense(Guid orgId, Guid productId, DateTime start, DateTime end, CancellationToken ct);

    public Task<Result<License>> UpdateLicenseDates(Guid id, DateTime start, DateTime end, CancellationToken ct);
    public Task<Result<License>> SuspendLicense(Guid id, CancellationToken ct);
    public Task<Result<License>> RevokeLicense(Guid id, CancellationToken ct);

    public Task<Result<bool>> SoftDeleteLicense(Guid id, CancellationToken ct);
}
