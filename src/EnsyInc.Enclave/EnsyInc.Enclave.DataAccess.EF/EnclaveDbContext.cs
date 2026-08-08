using EnsyInc.Enclave.DataAccess.EF.Configuration;
using EnsyInc.Enclave.DataAccess.Models;

using Microsoft.EntityFrameworkCore;

namespace EnsyInc.Enclave.DataAccess.EF;

public sealed class EnclaveDbContext : DbContext
{
    public DbSet<DbApp> Apps { get; init; }
    public DbSet<LicenseEntity> Licenses { get; init; }
    public DbSet<LicenseRequestEntity> LicenseRequests { get; init; }
    public DbSet<OrgEntity> Orgs { get; init; }
    public DbSet<ProductEntity> Products { get; init; }
    public DbSet<UserEntity> Users { get; init; }

    public EnclaveDbContext(DbContextOptions<EnclaveDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<DbApp>().Configure();
        modelBuilder.Entity<LicenseEntity>().Configure();
        modelBuilder.Entity<LicenseRequestEntity>().Configure();
        modelBuilder.Entity<OrgEntity>().Configure();
        modelBuilder.Entity<ProductEntity>().Configure();
        modelBuilder.Entity<UserEntity>().Configure();
    }
}
