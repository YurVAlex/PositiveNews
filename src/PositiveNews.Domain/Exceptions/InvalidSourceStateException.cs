namespace PositiveNews.Domain.Exceptions;

/// <summary>
/// Thrown when source aggregate rules are violated (empty name/url, invalid trust, etc.).
/// </summary>
public sealed class InvalidSourceStateException : DomainException
{
    /// <inheritdoc cref="DomainException.DomainException(string)" />
    public InvalidSourceStateException(string message) : base(message) { }
}
