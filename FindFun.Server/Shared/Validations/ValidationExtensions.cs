using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;

namespace FindFun.Server.Shared.Validations;

public static class ProblemDetailsResultExtensions
{
    // this is not working for records without property setters, but it is working for classes and records with property setters.
    // I will consier posible solutions to ensure same behaviour for record without properties.
    public static Result<T> ValidateWithProblemDetails<T>(this T obj, bool includeAllErrors = default)
    {
        var context = new ValidationContext(obj!);
        var results = new List<ValidationResult>();
        bool isValid = Validator.TryValidateObject(obj!, context, results, includeAllErrors);

        if (isValid)
            return Result<T>.Success();

        return BuildValidationProblemResult<T>(includeAllErrors, results);
    }

    private static Result<T> BuildValidationProblemResult<T>(bool includeAllErrors, List<ValidationResult> results)
    {
        // maybe the use of the ModelStateDictionary is not the best option because we can add the errors directly to the ValidationProblemDetails,
        var errorsToProcess = includeAllErrors ? results : results.Take(1);

        var errors = errorsToProcess
        .SelectMany(result => result.MemberNames
        .Select(member => (member, message: result.ErrorMessage ?? string.Empty)))
        .GroupBy(x => x.member, x => x.message)
        .ToDictionary(g => g.Key, g => g.ToArray());

        var problemDetails = new ValidationProblemDetails(errors)
        {
            Title = "One or more validation errors occurred.",
            Status = StatusCodes.Status400BadRequest,
            Type = ProblemDetailsConstants.BadRequest,
        };

        return Result<T>.Failure(problemDetails);
    }

    public static Result<TResult> CreateProblemResult<T, TResult>(string fieldName, string errorMessage, int statusCode = StatusCodes.Status404NotFound)
    {
        var (title, type) = GetProblemDetailsTitleAndType(statusCode);
        var problemDetails = new ValidationProblemDetails
        {
            Title = title,
            Status = statusCode,
            Type = type
        };
        problemDetails.Errors.Add(fieldName, [errorMessage]);

        return Result<TResult>.Failure(problemDetails);
    }

    private static (string Title, string Type) GetProblemDetailsTitleAndType(int statusCode)
    {
        // this all could be done in the GetProblemDetailsType method, with other constante for the title to avoid that call to the ReasonPhrases.GetReasonPhrase
        return (ReasonPhrases.GetReasonPhrase(statusCode), GetProblemDetailsType(statusCode));
    }

    private static string GetProblemDetailsType(int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status404NotFound => ProblemDetailsConstants.NotFound,
            StatusCodes.Status409Conflict => ProblemDetailsConstants.Conflict,
            StatusCodes.Status400BadRequest => ProblemDetailsConstants.BadRequest,
            StatusCodes.Status422UnprocessableEntity => ProblemDetailsConstants.UnprocessableEntity,
            StatusCodes.Status403Forbidden => ProblemDetailsConstants.Forbidden,
            StatusCodes.Status401Unauthorized => ProblemDetailsConstants.Unauthorized,
            _ => ProblemDetailsConstants.Default
        };
    }

    public static Result<TResult> CreateProblemResult<T, TResult>(this int statusCode, string fieldName, string errorMessage)
        => CreateProblemResult<T, TResult>(fieldName, errorMessage, statusCode);
}
