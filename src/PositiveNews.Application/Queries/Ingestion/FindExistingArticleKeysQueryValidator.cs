using FluentValidation;

namespace PositiveNews.Application.Queries.Ingestion;

/// <summary>
/// Ensures collection arguments are non-null (individual entries may still be empty).
/// </summary>
public sealed class FindExistingArticleKeysQueryValidator : AbstractValidator<FindExistingArticleKeysQuery>
{
    /// <summary>
    /// Initializes validation rules for <see cref="FindExistingArticleKeysQuery"/>.
    /// </summary>
    public FindExistingArticleKeysQueryValidator()
    {
        RuleFor(x => x.ExternalIds).NotNull();
        RuleFor(x => x.Urls).NotNull();
        RuleFor(x => x.Titles).NotNull();
    }
}
