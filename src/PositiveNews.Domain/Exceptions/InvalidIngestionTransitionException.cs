using PositiveNews.Domain.Enums;

namespace PositiveNews.Domain.Exceptions;

/// <summary>
/// Thrown when an <see cref="Entities.IngestionRun"/> state change is not allowed (e.g. not Running).
/// </summary>
public sealed class InvalidIngestionTransitionException : DomainException
{
    /// <summary>
    /// Describes the illegal transition using current and requested states.
    /// </summary>
    public InvalidIngestionTransitionException(IngestionStatus from, IngestionStatus to)
        : base($"Cannot transition IngestionRun from '{from}' to '{to}'.") { }
}
