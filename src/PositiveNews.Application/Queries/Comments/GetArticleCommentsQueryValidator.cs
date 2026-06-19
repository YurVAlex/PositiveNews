using FluentValidation;

namespace PositiveNews.Application.Queries.Comments;

/// <summary>
/// Validates <see cref="GetArticleCommentsQuery"/> identifiers.
/// </summary>
public sealed class GetArticleCommentsQueryValidator : AbstractValidator<GetArticleCommentsQuery>
{
    /// <summary>
    /// Initializes validation rules for <see cref="GetArticleCommentsQuery"/>.
    /// </summary>
    public GetArticleCommentsQueryValidator()
    {
        RuleFor(x => x.ArticleId).GreaterThan(0);
    }
}
