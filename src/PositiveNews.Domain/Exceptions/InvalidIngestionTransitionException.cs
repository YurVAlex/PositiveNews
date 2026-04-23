using PositiveNews.Domain.Enums;

namespace PositiveNews.Domain.Exceptions;

public sealed class InvalidIngestionTransitionException : DomainException
{
    public InvalidIngestionTransitionException(IngestionStatus from, IngestionStatus to)
        : base($"Cannot transition IngestionRun from '{from}' to '{to}'.") { }
}
