using EnsyInc.Enclave.Services.Abstractions;
using EnsyInc.Enclave.Services.Implementations;

using Microsoft.Extensions.DependencyInjection;

namespace EnsyInc.Enclave.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        => services
            .AddScoped<IProductsService, ProductsService>()
            .AddScoped<IOrgsService, OrgsService>()
            .AddScoped<IUsersService, UsersService>()
            .AddScoped<ILicensesService, LicensesService>()
            .AddScoped<ILicenseRequestsService, LicenseRequestsService>();
}
