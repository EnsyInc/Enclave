using EnsyInc.Enclave.DataAccess.Models;

using EnsyNet.DataAccess.EntityFramework.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnsyInc.Enclave.DataAccess.EF.Configuration;

internal static class OrgConfig
{
    public static void Configure(this EntityTypeBuilder<OrgEntity> builder)
    {
        builder.ConfigureBaseProperties();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.HasOne<UserEntity>()
            .WithMany()
            .HasForeignKey(e => e.PrimaryUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
