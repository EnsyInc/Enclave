using EnsyInc.Enclave.DataAccess.Models;

using EnsyNet.DataAccess.EntityFramework.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnsyInc.Enclave.DataAccess.EF.Configuration;

internal static class ProductConfig
{
    public static void Configure(this EntityTypeBuilder<ProductEntity> builder)
    {
        builder.ConfigureBaseProperties();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.Description)
            .HasMaxLength(1024);

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.HasIndex(e => e.Name)
            .IsUnique()
            .HasFilter("[DeletedAt] IS NULL");
    }
}
