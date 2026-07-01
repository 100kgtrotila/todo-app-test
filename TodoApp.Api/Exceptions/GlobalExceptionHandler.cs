using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TodoApp.Application.Common;

namespace TodoApp.Api.Exceptions;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            ServiceException se => se.ErrorType switch
            {
                ServiceErrorType.NotFound   => (StatusCodes.Status404NotFound,   "Not Found"),
                ServiceErrorType.Forbidden  => (StatusCodes.Status403Forbidden,  "Forbidden"),
                ServiceErrorType.Conflict   => (StatusCodes.Status409Conflict,   "Conflict"),
                ServiceErrorType.Validation => (StatusCodes.Status400BadRequest, "Bad Request"),
                _                           => (StatusCodes.Status500InternalServerError, "Internal Server Error")
            },
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };

        if (exception is ServiceException)
            logger.LogInformation("Business rule violation: {Message}", exception.Message);
        else
            logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

        var problemDetails = new ProblemDetails
        {
            Status   = statusCode,
            Title    = title,
            Detail   = exception.Message,
            Instance = httpContext.Request.Path
        };
        problemDetails.Extensions["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
