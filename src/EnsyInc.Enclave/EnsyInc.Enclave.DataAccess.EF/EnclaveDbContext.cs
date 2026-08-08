using EnsyInc.Enclave.DataAccess.EF.Configuration;
using EnsyInc.Enclave.DataAccess.Models;

using Microsoft.EntityFrameworkCore;

namespace EnsyInc.Enclave.DataAccess.EF;

public sealed class EnclaveDbContext : DbContext
{
    public DbSet<LicenseEntity> Licenses { get; init; }
    public DbSet<LicenseRequestEntity> LicenseRequests { get; init; }
    public DbSet<OrgEntity> Orgs { get; init; }
    public DbSet<ProductEntity> Products { get; init; }
    public DbSet<UserEntity> Users { get; init; }

    public EnclaveDbContext(DbContextOptions<EnclaveDbContext> options) : base(options)
    {
        // Every mutation in this codebase goes through ExecuteUpdateAsync/ExecuteDeleteAsync
        // (see BaseRepository), which bypass the change tracker entirely. Tracking query results
        // therefore only risks returning stale, already-tracked entities on a second read within
        // the same scoped DbContext after such a bypass-mutation, with no corresponding upside
        // (nothing here relies on SaveChanges-based tracked updates), so it's disabled outright.
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<LicenseEntity>().Configure();
        modelBuilder.Entity<LicenseRequestEntity>().Configure();
        modelBuilder.Entity<OrgEntity>().Configure();
        modelBuilder.Entity<ProductEntity>().Configure();
        modelBuilder.Entity<UserEntity>().Configure();
    }
}
