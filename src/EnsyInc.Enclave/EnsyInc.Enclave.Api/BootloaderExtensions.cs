using NLog;
using NLog.Extensions.Logging;

namespace EnsyInc.Enclave.Api;

internal static class BootloaderExtensions
{
    public static WebApplicationBuilder InitializeApplication(this WebApplicationBuilder builder)
    {
        builder.Configuration.InitializeConfiguration(builder.Environment.EnvironmentName);
        builder.Logging.ConfigureLogging();
        
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

    public static WebApplication ConfigureApplication(this WebApplication app)
    {
        return app;
    }
}
