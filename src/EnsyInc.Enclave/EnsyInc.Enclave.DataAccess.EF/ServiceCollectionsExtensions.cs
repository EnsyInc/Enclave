using EnsyInc.Enclave.Core.Config;
using EnsyInc.Enclave.DataAccess.Abstractions;
using EnsyInc.Enclave.DataAccess.EF.Implementations;

using EnsyNet.Core.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnsyInc.Enclave.DataAccess.EF;

public static class ServiceCollectionsExtensions
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration config)
    {
        services.AddRequiredConfiguration<DbConfig>(config, out var dbConfig);
        var dbContextOptions = GetDbContextOptions(dbConfig);
        services.AddSingleton(dbContextOptions);

        services.AddDbContextFactory<EnclaveDbContext>();

        return services.AddRepos();
    }

    private static IServiceCollection AddRepos(this IServiceCollection services)
        => services.AddScoped<IAppRepository, AppRepository>();

    private static DbContextOptions<EnclaveDbContext> GetDbContextOptions(DbConfig dbConfig)
        => new DbContextOptionsBuilder<EnclaveDbContext>()
            .UseSqlServer(dbConfig.ConnectionString, opts =>
            {
                opts.MigrationsAssembly("EnsyInc.Enclave.Migrations")
                    .EnableRetryOnFailure();
            }).Options;
}
