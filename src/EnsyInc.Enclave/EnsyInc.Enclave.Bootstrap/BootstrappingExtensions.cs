using System.Reflection;
using System.Text.Json.Serialization;

using EnsyInc.Enclave.DataAccess.EF;
using EnsyInc.Enclave.Services;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;

using NLog;
using NLog.Extensions.Logging;

namespace EnsyInc.Enclave.Bootstrap;

public static class BootstrappingExtensions
{
    extension(WebApplicationBuilder builder)
    {
        public WebApplicationBuilder InitializeApplication()
        {
            builder.Configuration.InitializeConfiguration(builder.Environment.EnvironmentName);
            builder.Logging.ConfigureLogging();
            builder.Services.AddServices(builder.Configuration);

            return builder;
        }
    }

    extension(ConfigurationManager config)
    {
        private void InitializeConfiguration(string envName)
        {
            config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
            config.AddJsonFile($"appsettings.{envName}.json", optional: true, reloadOnChange: false);
            config.AddEnvironmentVariables();
            var secretsFolder = config["SECRETS_FOLDER"];
            if (!string.IsNullOrWhiteSpace(secretsFolder))
            {
                config.AddKeyPerFile(secretsFolder, optional: true, reloadOnChange: false);
            }
        }
    }

    extension(ILoggingBuilder builder)
    {
        private void ConfigureLogging()
        {
            builder.ClearProviders();
            builder.AddNLog();
            LogManager.Setup().LoadConfigurationFromFile("nlog.config");
        }
    }

    extension(IServiceCollection services)
    {
        private void AddServices(IConfiguration config)
            => services.AddDefaultServices()
                .AddDataAccess(config)
                .AddApplicationServices();

        private IServiceCollection AddDefaultServices()
        {
            services.AddControllers()
                .AddJsonOptions(opt => opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
            services.AddOpenApi()
                .AddEndpointsApiExplorer()
                .AddSwaggerGen(opt =>
                {
                    opt.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Title = "EnsyInc.Enclave — Licensing API",
                        Version = "v1",
                        Description = "Admin and customer backoffice endpoints for the Licensing service.",
                    });

                    var xmlFilename = $"{Assembly.GetEntryAssembly()!.GetName().Name}.xml";
                    opt.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
                });
            return services;
        }
    }

    extension(WebApplication app)
    {
        public WebApplication ConfigureApplication()
            => app.AddDefaultAppPipeline();

        private WebApplication AddDefaultAppPipeline()
        {
            app.UseExceptionHandler();
            app.MapOpenApi();
            app.UseSwagger()
                .UseSwaggerUI();
            app.UseHttpsRedirection()
                .UseAuthorization();
            app.MapControllers();

            return app;
        }

        public void RunApplication()
        {
            try
            {
                app.Run();
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Fatal(ex, "An error occurred in the application");
                LogManager.Flush();
                LogManager.Shutdown();
                Environment.Exit(1);
            }

            LogManager.Flush();
            LogManager.Shutdown();
        }
    }
}
