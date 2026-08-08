using EnsyInc.Enclave.DataAccess.Models;

using EnsyNet.DataAccess.EntityFramework.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnsyInc.Enclave.DataAccess.EF.Configuration;

internal static class LicenseRequestConfig
{
    public static void Configure(this EntityTypeBuilder<LicenseRequestEntity> builder)
    {
        builder.ConfigureBaseProperties();

        builder.Property(e => e.RequestNotes)
            .HasMaxLength(2048);

        builder.Property(e => e.RejectionReason)
            .HasMaxLength(1024);

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

        builder.HasOne<UserEntity>()
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<LicenseEntity>()
            .WithMany()
            .HasForeignKey(e => e.ExistingLicenseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.Status)
            .HasFilter("[DeletedAt] IS NULL");
    }
}
