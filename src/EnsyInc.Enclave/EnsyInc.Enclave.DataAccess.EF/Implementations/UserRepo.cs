using EnsyInc.Enclave.DataAccess.Abstractions;
using EnsyInc.Enclave.DataAccess.Models;

using EnsyNet.DataAccess.EntityFramework;

using Microsoft.Extensions.Logging;

namespace EnsyInc.Enclave.DataAccess.EF.Implementations;

internal sealed class UserRepo : BaseRepository<UserEntity>, IUserRepo
{
    public UserRepo(EnclaveDbContext dbContext, ILogger<UserRepo> logger) : base(dbContext, dbContext.Users, logger) { }
}
