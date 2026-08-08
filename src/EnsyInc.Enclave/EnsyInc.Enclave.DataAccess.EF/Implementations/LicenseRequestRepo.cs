using EnsyInc.Enclave.DataAccess.Abstractions;
using EnsyInc.Enclave.DataAccess.Models;

using EnsyNet.DataAccess.EntityFramework;

using Microsoft.Extensions.Logging;

namespace EnsyInc.Enclave.DataAccess.EF.Implementations;

internal sealed class LicenseRequestRepo : BaseRepository<LicenseRequestEntity>, ILicenseRequestRepo
{
    public LicenseRequestRepo(EnclaveDbContext dbContext, ILogger<LicenseRequestRepo> logger) : base(dbContext, dbContext.LicenseRequests, logger) { }
}
