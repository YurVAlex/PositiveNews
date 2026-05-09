using Microsoft.AspNetCore.Mvc;
using PositiveNews.Application.Common;

namespace PositiveNews.Web.Api;

public static class ResultExtensions
{
    public static ActionResult<T> ToActionResult<T>(this Result<T> result, ControllerBase controller)
    {
        if (result.IsSuccess)
        {
            return controller.Ok(result.Value);
        }

        return new ObjectResult(new ProblemDetails
        {
            Title = result.Error.Code,
            Detail = result.Error.Message,
            Status = MapFailureErrorTypeToStatusCode(result.Error.Type)
        })
        {
            StatusCode = MapFailureErrorTypeToStatusCode(result.Error.Type)
        };
    }

    public static IActionResult ToActionResult(this Result result, ControllerBase controller)
    {
        if (result.IsSuccess)
        {
            return controller.Ok();
        }

        return new ObjectResult(new ProblemDetails
        {
            Title = result.Error.Code,
            Detail = result.Error.Message,
            Status = MapFailureErrorTypeToStatusCode(result.Error.Type)
        })
        {
            StatusCode = MapFailureErrorTypeToStatusCode(result.Error.Type)
        };
    }

    private static int MapFailureErrorTypeToStatusCode(ErrorType type) => type switch
    {
        ErrorType.None => throw new InvalidOperationException(
            "ErrorType.None is only used for successful results; it must not be mapped to an HTTP error status."),
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Unexpected => StatusCodes.Status500InternalServerError,
        _ => StatusCodes.Status500InternalServerError
    };
}
