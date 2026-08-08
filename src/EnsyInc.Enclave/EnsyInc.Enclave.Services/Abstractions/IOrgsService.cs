using EnsyInc.Enclave.Core.Models;

using EnsyNet.Core.Results;

namespace EnsyInc.Enclave.Services.Abstractions;

public interface IOrgsService
{
    public Task<Result<IEnumerable<Org>>> ListOrgs(CancellationToken ct);
    public Task<Result<IEnumerable<Org>>> ListOrgsByName(string name, CancellationToken ct);
    public Task<Result<Org?>> GetOrg(Guid id, CancellationToken ct);

    public Task<Result<Org>> CreateOrg(Org org);

    public Task<Result<Org>> UpdateOrg(Org org, CancellationToken ct);
    public Task<Result<Org>> DeactivateOrg(Guid id, CancellationToken ct);
    public Task<Result<Org>> ReactivateOrg(Guid id, CancellationToken ct);

    public Task<Result<bool>> SoftDeleteOrg(Guid id, CancellationToken ct);
}
