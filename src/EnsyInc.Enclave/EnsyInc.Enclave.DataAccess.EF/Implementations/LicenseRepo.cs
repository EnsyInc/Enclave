using EnsyInc.Enclave.DataAccess.Abstractions;
using EnsyInc.Enclave.DataAccess.Models;

using EnsyNet.DataAccess.EntityFramework;

using Microsoft.Extensions.Logging;

namespace EnsyInc.Enclave.DataAccess.EF.Implementations;

internal sealed class LicenseRepo : BaseRepository<LicenseEntity>, ILicenseRepo
{
    public LicenseRepo(EnclaveDbContext dbContext, ILogger<LicenseRepo> logger) : base(dbContext, dbContext.Licenses, logger) { }
}
