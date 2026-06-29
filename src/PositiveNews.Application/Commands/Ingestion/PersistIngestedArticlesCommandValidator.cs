using FluentValidation;

namespace PositiveNews.Application.Commands.Ingestion;

/// <summary>
/// Requires a positive source id, non-empty language code, and a non-null item collection.
/// </summary>
public sealed class PersistIngestedArticlesCommandValidator : AbstractValidator<PersistIngestedArticlesCommand>
{
    /// <summary>
    /// Initializes validation rules for <see cref="PersistIngestedArticlesCommand"/>.
    /// </summary>
    public PersistIngestedArticlesCommandValidator()
    {
        RuleFor(x => x.SourceId).GreaterThan(0);
        RuleFor(x => x.DefaultLanguageCode).NotEmpty();
        RuleFor(x => x.TopicLookup).NotNull();
        RuleFor(x => x.Items).NotNull();
    }
}
