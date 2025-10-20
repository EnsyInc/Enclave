using EnsyInc.Enclave.Api;

var builder = WebApplication.CreateBuilder(args)
    .InitializeApplication();

var app = builder.Build()
    .ConfigureApplication();

app.RunApplication();

