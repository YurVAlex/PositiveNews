namespace PositiveNews.Domain.Exceptions;

/// <summary>
/// Thrown when article aggregate rules are violated (missing fields, bad scores, invalid transitions).
/// </summary>
public sealed class InvalidArticleStateException : DomainException
{
    /// <inheritdoc cref="DomainException.DomainException(string)" />
    public InvalidArticleStateException(string message) : base(message) { }
}
