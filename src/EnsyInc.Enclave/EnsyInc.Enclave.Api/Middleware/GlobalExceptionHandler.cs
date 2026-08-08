using EnsyInc.Enclave.Api.Exceptions;
using EnsyInc.Enclave.Api.Models;

using FluentValidation;

using Microsoft.AspNetCore.Diagnostics;

namespace EnsyInc.Enclave.Api.Middleware;

internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        if (exception is ValidationException validationException)
        {
            logger.LogWarning(validationException, "Validation failed while processing {Method} {Path}.", httpContext.Request.Method, httpContext.Request.Path);

            var parameters = validationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => string.Join(" ", g.Select(e => e.ErrorMessage)));

            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(ErrorResponses.ValidationError(parameters), ct);

            return true;
        }

        if (exception is UnhandledResultErrorException)
        {
            logger.LogError(exception, "Controller reached an unhandled result error case while processing {Method} {Path}.", httpContext.Request.Method, httpContext.Request.Path);
        }
        else
        {
            logger.LogError(exception, "Unhandled exception while processing {Method} {Path}.", httpContext.Request.Method, httpContext.Request.Path);
        }

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(ErrorResponses.UnexpectedError, ct);

        return true;
    }
}
