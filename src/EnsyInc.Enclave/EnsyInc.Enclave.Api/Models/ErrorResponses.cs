namespace EnsyInc.Enclave.Api.Models;

internal static class ErrorResponses
{
    public static ErrorResponse ProductNotFoundError = new ErrorResponse("ProductNotFound", "The requested product was not found.", []);

    public static ErrorResponse UnexpectedError = new ErrorResponse("UnexpectedError", "An unexpected error occurred.", []);

    public static ErrorResponse ValidationError(Dictionary<string, string> parameters)
        => new("ValidationError", "One or more fields failed validation.", parameters);
}
