using FluentValidation;

namespace PositiveNews.Application.Queries.Articles;

/// <summary>
/// Requires a positive article identifier.
/// </summary>
public sealed class GetArticleDetailQueryValidator : AbstractValidator<GetArticleDetailQuery>
{
    /// <summary>
    /// Initializes validation rules for <see cref="GetArticleDetailQuery"/>.
    /// </summary>
    public GetArticleDetailQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
