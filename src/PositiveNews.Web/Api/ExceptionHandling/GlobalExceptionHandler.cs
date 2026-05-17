using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using PositiveNews.Domain.Exceptions;

namespace PositiveNews.Web.Api.ExceptionHandling;

/// <summary>
/// Central exception handler that converts validation, domain, and unexpected errors into RFC 7807 responses.
/// </summary>
/// <param name="problemDetailsService">Writes standardized problem details to the HTTP response.</param>
/// <param name="environment">Used to decide whether to expose exception messages in development.</param>
/// <param name="logger">Logs unhandled exceptions.</param>
public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    IHostEnvironment environment,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    /// <summary>
    /// Attempts to handle the given exception and write an appropriate problem details response.
    /// </summary>
    /// <param name="httpContext">The current HTTP context.</param>
    /// <param name="exception">The exception thrown during request processing.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>
    /// <see langword="true"/> if the exception was handled and the response was written; otherwise <see langword="false"/>.
    /// </returns>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        return exception switch
        {
            ValidationException vex => await WriteValidationAsync(httpContext, vex),
            DomainException dex => await WriteDomainAsync(httpContext, dex),
            _ => await WriteUnexpectedAsync(httpContext, exception)
        };
    }

    /// <summary>
    /// Emits a 400 validation problem details payload grouped by property name.
    /// </summary>
    /// <param name="httpContext">The current HTTP context.</param>
    /// <param name="exception">The FluentValidation aggregate exception.</param>
    /// <returns>Always <see langword="true"/> after writing the response.</returns>
    private async Task<bool> WriteValidationAsync(
        HttpContext httpContext,
        ValidationException exception)
    {
        var errors = exception.Errors
            .GroupBy(e => string.IsNullOrEmpty(e.PropertyName) ? string.Empty : e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        var problemDetails = new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred.",
            Detail = "See the errors field for details."
        };

        ProblemDetailsTraceExtensions.EnrichWithTrace(httpContext, problemDetails);

        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception
        });

        return true;
    }

    /// <summary>
    /// Emits a 400 problem details response for domain rule violations.
    /// </summary>
    /// <param name="httpContext">The current HTTP context.</param>
    /// <param name="exception">The domain exception carrying a user-safe message.</param>
    /// <returns>Always <see langword="true"/> after writing the response.</returns>
    private async Task<bool> WriteDomainAsync(
        HttpContext httpContext,
        DomainException exception)
    {
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Bad Request",
            Detail = exception.Message,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
        };

        ProblemDetailsTraceExtensions.EnrichWithTrace(httpContext, problemDetails);

        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception
        });

        return true;
    }

    /// <summary>
    /// Emits a 500 problem details response and logs the unexpected exception.
    /// </summary>
    /// <param name="httpContext">The current HTTP context.</param>
    /// <param name="exception">The unhandled exception.</param>
    /// <returns>Always <see langword="true"/> after writing the response.</returns>
    private async Task<bool> WriteUnexpectedAsync(
        HttpContext httpContext,
        Exception exception)
    {
        logger.LogError(exception, "Unhandled exception.");

        var detail = environment.IsDevelopment()
            ? exception.Message
            : "An unexpected error occurred.";

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An error occurred while processing your request.",
            Detail = detail,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
        };

        ProblemDetailsTraceExtensions.EnrichWithTrace(httpContext, problemDetails);

        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception
        });

        return true;
    }
}
