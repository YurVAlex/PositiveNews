namespace PositiveNews.Domain.Exceptions;

/// <summary>
/// Base exception for domain rule violations (invalid aggregates, transitions, or invariants).
/// </summary>
public class DomainException : Exception
{
    /// <summary>Creates an exception with a user-visible message.</summary>
    public DomainException(string message) : base(message) { }

    /// <summary>Creates an exception wrapping an inner cause.</summary>
    public DomainException(string message, Exception inner) : base(message, inner) { }
}
