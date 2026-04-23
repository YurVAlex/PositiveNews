namespace PositiveNews.Domain.Exceptions;

public sealed class InvalidArticleStateException : DomainException
{
    public InvalidArticleStateException(string message) : base(message) { }
}
