using ErrorOr;
using Microsoft.AspNetCore.Mvc;

namespace TodoApp.Api.Extensions;

public static class ErrorOrExtensions
{
    public static IActionResult ToProblem(this List<Error> errors, ControllerBase controller)
    {
        if (errors.Count is 0)
        {
            return controller.Problem();
        }

        var firstError = errors[0];

        var (statusCode, title) = firstError.Type switch
        {
            ErrorType.Validation => (StatusCodes.Status400BadRequest, "Bad Request"),
            ErrorType.Unauthorized => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            ErrorType.Forbidden => (StatusCodes.Status403Forbidden, "Forbidden"),
            ErrorType.NotFound => (StatusCodes.Status404NotFound, "Not Found"),
            ErrorType.Conflict => (StatusCodes.Status409Conflict, "Conflict"),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };

        return controller.Problem(
            statusCode: statusCode,
            title: title,
            detail: firstError.Description);
    }
}
