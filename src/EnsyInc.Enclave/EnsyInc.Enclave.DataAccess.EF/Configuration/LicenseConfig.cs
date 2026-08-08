using EnsyInc.Enclave.DataAccess.Models;

using EnsyNet.DataAccess.EntityFramework.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnsyInc.Enclave.DataAccess.EF.Configuration;

internal static class LicenseConfig
{
    public static void Configure(this EntityTypeBuilder<LicenseEntity> builder)
    {
        builder.ConfigureBaseProperties();

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.HasOne<OrgEntity>()
            .WithMany()
            .HasForeignKey(e => e.OrgId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ProductEntity>()
            .WithMany()
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
