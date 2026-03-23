using FindFun.Server.Shared.Resources;
using FindFun.Server.Shared.Validations;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;

namespace FindFun.Server.Shared.Middleware;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IWebHostEnvironment env) : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;
    private readonly IWebHostEnvironment _env = env;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, Messages.UnhandledExceptionOccurred);
        var problem = CreateProblemDetails(exception);

        await Results.Problem(problem).ExecuteAsync(httpContext);
        return true;
    }

    private ValidationProblemDetails CreateProblemDetails(Exception exception)
    {
        (int statusCode, string type, string title) = exception switch
        {
            // will add some constants to avoid calling GetReasonPhrase every time
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, ProblemDetailsConstants.Unauthorized, ReasonPhrases.GetReasonPhrase(StatusCodes.Status401Unauthorized)),
            ArgumentException or BadHttpRequestException => (StatusCodes.Status400BadRequest, ProblemDetailsConstants.BadRequest, ReasonPhrases.GetReasonPhrase(StatusCodes.Status400BadRequest)),
            ValidationException => (StatusCodes.Status400BadRequest, ProblemDetailsConstants.UnprocessableEntity, ReasonPhrases.GetReasonPhrase(StatusCodes.Status422UnprocessableEntity)),
            KeyNotFoundException => (StatusCodes.Status404NotFound, ProblemDetailsConstants.NotFound, ReasonPhrases.GetReasonPhrase(StatusCodes.Status404NotFound)),
            _ => (StatusCodes.Status500InternalServerError, ProblemDetailsConstants.InternalServer, ReasonPhrases.GetReasonPhrase(StatusCodes.Status500InternalServerError))
        };
        var problemDetail = new ValidationProblemDetails
        {
            Type = type,
            Title = title,
            Status = statusCode,
        };
        if (_env.IsDevelopment())
            problemDetail.Errors.Add(exception.GetType().Name, [exception.Message]);

        return problemDetail;
    }
}
