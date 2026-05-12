using FluentValidation;

namespace PositiveNews.Application.Commands.Ingestion;

/// <summary>
/// Ensures the source snapshot and nested fields are present and valid identifiers are positive.
/// </summary>
public sealed class ProcessIngestionSourceCommandValidator : AbstractValidator<ProcessIngestionSourceCommand>
{
    /// <summary>
    /// Initializes validation rules for <see cref="ProcessIngestionSourceCommand"/>.
    /// </summary>
    public ProcessIngestionSourceCommandValidator()
    {
        RuleFor(x => x.Source).NotNull();
        RuleFor(x => x.Source.Id).GreaterThan(0);
        RuleFor(x => x.Source.Name).NotEmpty();
        RuleFor(x => x.Source.FeedUrl).NotEmpty();
        RuleFor(x => x.Source.DefaultLanguageCode).NotEmpty();
        RuleFor(x => x.TopicLookup).NotNull();
        RuleFor(x => x.IngestionSettings).NotNull();
    }
}
