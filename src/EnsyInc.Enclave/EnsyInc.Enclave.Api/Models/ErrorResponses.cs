namespace EnsyInc.Enclave.Api.Models;

internal static class ErrorResponses
{
    public static readonly ErrorResponse ProductNotFoundError = new("ProductNotFound", "The requested product was not found.", []);

    public static readonly ErrorResponse OrgNotFoundError = new("OrgNotFound", "The requested org was not found.", []);

    public static readonly ErrorResponse UserNotFoundError = new("UserNotFound", "The requested user was not found.", []);

    public static readonly ErrorResponse UnexpectedError = new("UnexpectedError", "An unexpected error occurred.", []);

    public static ErrorResponse ValidationError(Dictionary<string, string> parameters)
        => new("ValidationError", "One or more fields failed validation.", parameters);
}
