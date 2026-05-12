namespace PositiveNews.Application.Common;

/// <summary>
/// Categories application failures for mapping to HTTP status codes and logging.
/// </summary>
public enum ErrorType
{
    /// <summary>No error (success).</summary>
    None,

    /// <summary>Input validation failed.</summary>
    Validation,

    /// <summary>Authentication required or failed.</summary>
    Unauthorized,

    /// <summary>Entity not found.</summary>
    NotFound,

    /// <summary>Business conflict (e.g. duplicate email).</summary>
    Conflict,

    /// <summary>Unexpected server-side failure.</summary>
    Unexpected
}

/// <summary>
/// Structured error payload carried inside failed <see cref="Result"/> or <see cref="Result{T}"/>.
/// </summary>
/// <param name="Code">Stable machine-readable error identifier.</param>
/// <param name="Message">Human-readable explanation suitable for clients or logs.</param>
/// <param name="Type">High-level category used for HTTP mapping and diagnostics.</param>
public sealed record Error(string Code, string Message, ErrorType Type)
{
    /// <summary>Sentinel value used by successful results.</summary>
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.None);
}

/// <summary>
/// Represents either success or failure without a typed payload (commands that return only status).
/// </summary>
public class Result
{
    /// <summary>Creates a success or failure result with the given error when failing.</summary>
    /// <param name="isSuccess">Whether the operation succeeded.</param>
    /// <param name="error">Error details when <paramref name="isSuccess"/> is <see langword="false"/>.</param>
    protected Result(bool isSuccess, Error error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>True when the operation completed without a domain/application error.</summary>
    public bool IsSuccess { get; }

    /// <summary>True when <see cref="Error"/> describes what went wrong.</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>Error details when <see cref="IsFailure"/> is true.</summary>
    public Error Error { get; }

    /// <summary>Successful void result.</summary>
    /// <returns>A success instance without payload.</returns>
    public static Result Success() => new(true, Error.None);

    /// <summary>Failed result with the given error.</summary>
    /// <param name="error">Structured failure details.</param>
    /// <returns>A failure instance carrying <paramref name="error"/>.</returns>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>Allows returning an <see cref="Error"/> where a <see cref="Result"/> is expected.</summary>
    /// <param name="error">Error to lift into a result.</param>
    /// <returns>A failed result wrapping <paramref name="error"/>.</returns>
    public static implicit operator Result(Error error) => Failure(error);

    /// <summary>Branches on success vs failure without exposing exceptions.</summary>
    /// <typeparam name="TResult">The mapped result type.</typeparam>
    /// <param name="onSuccess">Invoked when <see cref="IsSuccess"/> is <see langword="true"/>.</param>
    /// <param name="onFailure">Invoked with the error when <see cref="IsFailure"/> is <see langword="true"/>.</param>
    /// <returns>The value produced by either callback.</returns>
    public TResult Match<TResult>(Func<TResult> onSuccess, Func<Error, TResult> onFailure)
        => IsSuccess ? onSuccess() : onFailure(Error);
}

/// <summary>
/// Result carrying a value on success (e.g. query DTO or auth token bundle).
/// </summary>
public sealed class Result<T> : Result
{
    private readonly T? _value;

    private Result(T value) : base(true, Error.None)
    {
        _value = value;
    }

    private Result(Error error) : base(false, error)
    {
    }

    /// <summary>
    /// Successful payload; throws if this instance represents failure.
    /// </summary>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value for failed Result.");

    /// <summary>Creates a successful result wrapping <paramref name="value"/>.</summary>
    /// <param name="value">Successful payload.</param>
    /// <returns>A success instance containing <paramref name="value"/>.</returns>
    public static Result<T> Success(T value) => new(value);

    /// <summary>Creates a failed result with <paramref name="error"/>.</summary>
    /// <param name="error">Structured failure details.</param>
    /// <returns>A failure instance without a success value.</returns>
    public static new Result<T> Failure(Error error) => new(error);

    /// <summary>Lift a plain value into <see cref="Result{T}"/>.</summary>
    /// <param name="value">Implicit success payload.</param>
    /// <returns>A successful result wrapping <paramref name="value"/>.</returns>
    public static implicit operator Result<T>(T value) => Success(value);

    /// <summary>Lift an error into <see cref="Result{T}"/>.</summary>
    /// <param name="error">Implicit failure payload.</param>
    /// <returns>A failed result wrapping <paramref name="error"/>.</returns>
    public static implicit operator Result<T>(Error error) => Failure(error);

    /// <summary>Maps success or surfaces the existing error.</summary>
    /// <typeparam name="TResult">The mapped result type.</typeparam>
    /// <param name="onSuccess">Invoked with the success value when applicable.</param>
    /// <param name="onFailure">Invoked with the error when the result is a failure.</param>
    /// <returns>The value produced by either callback.</returns>
    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<Error, TResult> onFailure)
        => IsSuccess ? onSuccess(Value) : onFailure(Error);

    /// <summary>Transforms the success value; preserves failures.</summary>
    /// <typeparam name="TOut">The mapped success value type.</typeparam>
    /// <param name="mapper">Maps the current success value to <typeparamref name="TOut"/>.</param>
    /// <returns>A new result containing the mapped value, or the original failure.</returns>
    public Result<TOut> Map<TOut>(Func<T, TOut> mapper)
        => Match(
            value => Result<TOut>.Success(mapper(value)),
            Result<TOut>.Failure);

    /// <summary>Chains another result-producing step; short-circuits on failure.</summary>
    /// <typeparam name="TOut">The next success value type.</typeparam>
    /// <param name="binder">Produces the next result from the current success value.</param>
    /// <returns>The binder result, or the propagated failure.</returns>
    public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> binder)
        => Match(
            binder,
            Result<TOut>.Failure);
}
