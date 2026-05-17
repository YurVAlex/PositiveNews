using Microsoft.AspNetCore.Mvc;
using PositiveNews.Application.Common;

namespace PositiveNews.Web.Api;

/// <summary>
/// Maps application-layer <see cref="Result{T}"/> values to ASP.NET Core action results.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Converts a typed result into either an OK payload or an RFC 7807 problem response.
    /// </summary>
    /// <typeparam name="T">The successful payload type.</typeparam>
    /// <param name="result">The application operation result.</param>
    /// <param name="controller">The controller providing HTTP context for problem details.</param>
    /// <returns>An <see cref="ActionResult{TValue}"/> representing success or failure.</returns>
    /// <remarks>
    /// Failure responses include trace enrichment via <see cref="ProblemDetailsTraceExtensions.EnrichWithTrace"/>.
    /// </remarks>
    public static ActionResult<T> ToActionResult<T>(this Result<T> result, ControllerBase controller)
    {
        if (result.IsSuccess)
        {
            return controller.Ok(result.Value);
        }

        var problemDetails = new ProblemDetails
        {
            Title = result.Error.Code,
            Detail = result.Error.Message,
            Status = MapFailureErrorTypeToStatusCode(result.Error.Type)
        };
        ProblemDetailsTraceExtensions.EnrichWithTrace(controller.HttpContext, problemDetails);
        return new ObjectResult(problemDetails)
        {
            StatusCode = problemDetails.Status
        };
    }

    /// <summary>
    /// Converts a result without a payload into either HTTP 200 OK or an RFC 7807 problem response.
    /// </summary>
    /// <param name="result">The application operation result.</param>
    /// <param name="controller">The controller providing HTTP context for problem details.</param>
    /// <returns>An <see cref="IActionResult"/> representing success or failure.</returns>
    public static IActionResult ToActionResult(this Result result, ControllerBase controller)
    {
        if (result.IsSuccess)
        {
            return controller.Ok();
        }

        var problemDetails = new ProblemDetails
        {
            Title = result.Error.Code,
            Detail = result.Error.Message,
            Status = MapFailureErrorTypeToStatusCode(result.Error.Type)
        };
        ProblemDetailsTraceExtensions.EnrichWithTrace(controller.HttpContext, problemDetails);
        return new ObjectResult(problemDetails)
        {
            StatusCode = problemDetails.Status
        };
    }

    /// <summary>
    /// Maps domain error categories to HTTP status codes for problem details.
    /// </summary>
    /// <param name="type">The classified failure kind.</param>
    /// <returns>The HTTP status code to use for the response.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="type"/> is <see cref="ErrorType.None"/>.</exception>
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
