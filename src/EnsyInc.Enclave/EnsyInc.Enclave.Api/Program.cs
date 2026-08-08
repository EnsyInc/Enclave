using EnsyInc.Enclave.Api.Middleware;
using EnsyInc.Enclave.Api.Validators;
using EnsyInc.Enclave.Bootstrap;

using FluentValidation;

using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args)
    .InitializeApplication();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductRequestValidator>();
builder.Services.Configure<MvcOptions>(opt =>
    opt.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);

var app = builder.Build()
    .ConfigureApplication();

app.RunApplication();
