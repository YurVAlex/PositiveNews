using FluentValidation;
using MediatR;

namespace PositiveNews.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline step that runs FluentValidation validators before the handler.
/// Aggregates failures into a <see cref="Result"/>/<see cref="Result{T}"/> when possible; otherwise throws <see cref="ValidationException"/>.
/// </summary>
/// <param name="validators">FluentValidation validators registered for <typeparamref name="TRequest"/>.</param>
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <summary>
    /// Validates <paramref name="request"/>; on failure returns a validation error result or throws.
    /// </summary>
    /// <param name="request">The incoming MediatR request.</param>
    /// <param name="next">The delegate that invokes the next pipeline behavior or handler.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The handler response, or a validation failure when applicable.</returns>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count == 0)
        {
            return await next();
        }

        var message = string.Join(" ", failures.Select(f => f.ErrorMessage));
        var error = new Error("Validation.Failed", message, ErrorType.Validation);

        if (TryCreateResultFailure(error, out var response))
        {
            return response;
        }

        throw new ValidationException(failures);
    }

    /// <summary>
    /// When the MediatR response type is <see cref="Result"/> or <see cref="Result{T}"/>, builds a failure without throwing.
    /// </summary>
    private static bool TryCreateResultFailure(Error error, out TResponse response)
    {
        var responseType = typeof(TResponse);

        if (responseType == typeof(Result))
        {
            response = (TResponse)(object)Result.Failure(error);
            return true;
        }

        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var failureMethod = responseType.GetMethod(
                nameof(Result<int>.Failure),
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static,
                [typeof(Error)]);

            if (failureMethod is not null)
            {
                response = (TResponse)failureMethod.Invoke(null, [error])!;
                return true;
            }
        }

        response = default!;
        return false;
    }
}
