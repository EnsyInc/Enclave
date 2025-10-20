using EnsyInc.Enclave.DataAccess.Models;

using EnsyNet.DataAccess.EntityFramework.Configuration;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnsyInc.Enclave.DataAccess.EF.Configuration;

internal static class DbAppConfig
{
    public static void Configure(this EntityTypeBuilder<DbApp> builder)
    {
        builder.ConfigureBaseProperties();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.Description)
            .HasMaxLength(1024);
    }
}
