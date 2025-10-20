using NLog;
using NLog.Extensions.Logging;

using System.Reflection;

namespace EnsyInc.Enclave.Api;

internal static class BootloaderExtensions
{
    public static WebApplicationBuilder InitializeApplication(this WebApplicationBuilder builder)
    {
        builder.Configuration.InitializeConfiguration(builder.Environment.EnvironmentName);
        builder.Logging.ConfigureLogging();
        builder.Services.AddDefaultServices();
        
        return builder;
    }

    private static void InitializeConfiguration(this ConfigurationManager config, string envName)
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

    private static void ConfigureLogging(this ILoggingBuilder builder)
    {
        builder.ClearProviders();
        builder.AddNLog();
        LogManager.Setup().LoadConfigurationFromFile("nlog.config");
    }

    private static void AddDefaultServices(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddOpenApi()
            .AddEndpointsApiExplorer()
            .AddSwaggerGen(opt =>
            {
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                opt.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
            });   
    }

    public static WebApplication ConfigureApplication(this WebApplication app)
    {
        return app.AddDefaultAppPipeline();
    }

    private static WebApplication AddDefaultAppPipeline(this WebApplication app)
    {
        app.MapOpenApi();
        app.UseSwagger()
            .UseSwaggerUI();
        app.UseHttpsRedirection()
            .UseAuthorization();
        app.MapControllers();

        return app;
    }

    public static void RunApplication(this WebApplication app)
    {
        try
        {
            app.Run();
        }
#pragma warning disable CA1031
        catch (Exception ex)
#pragma warning restore CA1031
        {
            LogManager.GetCurrentClassLogger().Fatal(ex, "An error occurred in the application");
            LogManager.Flush();
            LogManager.Shutdown();
            Environment.Exit(1);
        }
    }
}
