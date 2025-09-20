using EnsyInc.Enclave.Api;

using NLog;

var builder = WebApplication.CreateBuilder(args)
    .InitializeApplication();

var app = builder.Build()
    .ConfigureApplication();

try
{
    app.Run();
}
catch (Exception ex)
{
    LogManager.GetCurrentClassLogger().Fatal(ex, "An error occurred in the application");
    LogManager.Shutdown();
    Environment.Exit(1);
}
finally
{
    LogManager.Shutdown();
}

//builder.Configuration.add
//builder.Services.AddControllers();
//builder.Services.AddOpenApi()
//    .AddEndpointsApiExplorer()
//    .AddSwaggerGen();

//app.MapOpenApi();
//app.UseSwagger()
//    .UseSwaggerUI();
//app.UseHttpsRedirection()
//    .UseAuthorization();
//app.MapControllers();

//return;
