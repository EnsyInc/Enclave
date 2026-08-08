using EnsyInc.Enclave.DataAccess.Abstractions;
using EnsyInc.Enclave.DataAccess.Models;

using EnsyNet.DataAccess.EntityFramework;

using Microsoft.Extensions.Logging;

namespace EnsyInc.Enclave.DataAccess.EF.Implementations;

internal sealed class OrgRepo : BaseRepository<OrgEntity>, IOrgRepo
{
    public OrgRepo(EnclaveDbContext dbContext, ILogger<OrgRepo> logger) : base(dbContext, dbContext.Orgs, logger) { }
}
