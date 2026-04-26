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
            Status = ToHttpStatusCode(result.Error.Type)
        })
        {
            StatusCode = ToHttpStatusCode(result.Error.Type)
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
            Status = ToHttpStatusCode(result.Error.Type)
        })
        {
            StatusCode = ToHttpStatusCode(result.Error.Type)
        };
    }

    private static int ToHttpStatusCode(ErrorType type) => type switch
    {
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status500InternalServerError
    };
}
