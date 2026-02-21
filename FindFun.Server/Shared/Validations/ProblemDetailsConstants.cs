namespace FindFun.Server.Shared.Validations;

public static class ProblemDetailsConstants
{
    public const string NotFound = "https://tools.ietf.org/html/rfc9110#section-15.5.5";
    public const string Conflict = "https://tools.ietf.org/html/rfc9110#section-15.5.10";
    public const string BadRequest = "https://tools.ietf.org/html/rfc9110#section-15.5.1";
    public const string UnprocessableEntity = "https://tools.ietf.org/html/rfc4918#section-11.2";
    public const string Forbidden = "https://tools.ietf.org/html/rfc9110#section-15.5.4";
    public const string Unauthorized = "https://tools.ietf.org/html/rfc9110#section-15.5.2";
    public const string InternalServer = "https://tools.ietf.org/html/rfc7231#section-6.6.1";
    public const string Default = BadRequest; // Default to Bad Request
}