using EnsyInc.Enclave.DataAccess.EF;

using Microsoft.EntityFrameworkCore;

var builder = Host.CreateDefaultBuilder(args);
builder.ConfigureServices((context, services) =>
{
    services.AddDataAccess(context.Configuration);
});

var app = builder.Build();

var dbContextFactory = app.Services.GetRequiredService<IDbContextFactory<EnclaveDbContext>>();
await using var dbContext = dbContextFactory.CreateDbContext();
await dbContext.Database.MigrateAsync();
