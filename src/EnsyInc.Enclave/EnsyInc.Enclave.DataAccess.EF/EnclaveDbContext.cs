using EnsyInc.Enclave.DataAccess.EF.Configuration;
using EnsyInc.Enclave.DataAccess.Models;

using Microsoft.EntityFrameworkCore;

namespace EnsyInc.Enclave.DataAccess.EF;

public sealed class EnclaveDbContext : DbContext
{
    public DbSet<DbApp> Apps { get; init; }

    public EnclaveDbContext(DbContextOptions<EnclaveDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<DbApp>().Configure();
    }
}
