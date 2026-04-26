using FluentValidation;

namespace PositiveNews.Application.Commands.Ingestion;

public sealed class PersistIngestedArticlesCommandValidator : AbstractValidator<PersistIngestedArticlesCommand>
{
    public PersistIngestedArticlesCommandValidator()
    {
        RuleFor(x => x.SourceId).GreaterThan(0);
        RuleFor(x => x.DefaultLanguageCode).NotEmpty();
        RuleFor(x => x.Items).NotNull();
    }
}
