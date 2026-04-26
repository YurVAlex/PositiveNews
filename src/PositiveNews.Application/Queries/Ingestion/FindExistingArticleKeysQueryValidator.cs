using FluentValidation;

namespace PositiveNews.Application.Queries.Ingestion;

public sealed class FindExistingArticleKeysQueryValidator : AbstractValidator<FindExistingArticleKeysQuery>
{
    public FindExistingArticleKeysQueryValidator()
    {
        RuleFor(x => x.ExternalIds).NotNull();
        RuleFor(x => x.Urls).NotNull();
        RuleFor(x => x.Titles).NotNull();
    }
}
