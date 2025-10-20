using EnsyInc.Enclave.DataAccess.Abstractions;
using EnsyInc.Enclave.DataAccess.Models;

using EnsyNet.DataAccess.EntityFramework;

using Microsoft.Extensions.Logging;

namespace EnsyInc.Enclave.DataAccess.EF.Implementations;

internal sealed class AppRepository : BaseRepository<DbApp>, IAppRepository
{
    public AppRepository(EnclaveDbContext dbContext, ILogger<AppRepository> logger) : base(dbContext, dbContext.Apps, logger) { }
}
