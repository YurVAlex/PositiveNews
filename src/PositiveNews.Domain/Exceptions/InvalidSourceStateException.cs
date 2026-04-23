namespace PositiveNews.Domain.Exceptions;

public sealed class InvalidSourceStateException : DomainException
{
    public InvalidSourceStateException(string message) : base(message) { }
}
