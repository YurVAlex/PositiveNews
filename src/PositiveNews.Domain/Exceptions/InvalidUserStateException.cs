namespace PositiveNews.Domain.Exceptions;

/// <summary>
/// Thrown when user aggregate rules are violated (empty identity fields, duplicate deactivate, etc.).
/// </summary>
public sealed class InvalidUserStateException : DomainException
{
    /// <inheritdoc cref="DomainException.DomainException(string)" />
    public InvalidUserStateException(string message) : base(message) { }
}
