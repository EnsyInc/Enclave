using EnsyInc.Enclave.DataAccess.Models;

using EnsyNet.DataAccess.EntityFramework.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnsyInc.Enclave.DataAccess.EF.Configuration;

internal static class UserConfig
{
    public static void Configure(this EntityTypeBuilder<UserEntity> builder)
    {
        builder.ConfigureBaseProperties();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(320);

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(e => e.Role)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.HasOne<OrgEntity>()
            .WithMany()
            .HasForeignKey(e => e.OrgId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.Email)
            .IsUnique()
            .HasFilter("[DeletedAt] IS NULL");
    }
}
