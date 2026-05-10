using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using PositiveNews.Domain.Exceptions;

namespace PositiveNews.Web.Api.ExceptionHandling;

public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    IHostEnvironment environment,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
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
