using FluentValidation;

namespace PositiveNews.Application.Queries.Articles;

/// <summary>
/// Ensures page and page size are in range, sort enum is defined, and at most 30 topics each ≤120 characters when provided.
/// </summary>
public sealed class GetArticleFeedQueryValidator : AbstractValidator<GetArticleFeedQuery>
{
    /// <summary>
    /// Initializes validation rules for <see cref="GetArticleFeedQuery"/>.
    /// </summary>
    public GetArticleFeedQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.SortBy).IsInEnum();
        RuleFor(x => x.Topics).Must(t => t == null || t.Count <= 30).WithMessage("At most 30 topics may be used for ordering.");
        When(x => x.Topics is { Count: > 0 }, () =>
        {
            RuleForEach(x => x.Topics!).MaximumLength(120);
        });
    }
}
