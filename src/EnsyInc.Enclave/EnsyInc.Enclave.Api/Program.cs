using EnsyInc.Enclave.Bootstrap;

var builder = WebApplication.CreateBuilder(args)
    .InitializeApplication();

var app = builder.Build()
    .ConfigureApplication();

app.RunApplication();
