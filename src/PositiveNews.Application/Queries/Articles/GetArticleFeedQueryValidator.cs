using FluentValidation;

namespace PositiveNews.Application.Queries.Articles;

/// <summary>
/// Ensures page and page size are in range, sort enum is defined, and topic filters are bounded and non-empty when provided.
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
        When(x => x.MinPositivity.HasValue, () =>
        {
            RuleFor(x => x.MinPositivity!.Value).InclusiveBetween(0m, 1m);
        });
        RuleFor(x => x.Topics)
            .Must(t => t == null || t.Count <= 30)
            .WithMessage("At most 30 topics may be used for filtering.");
        When(x => x.Topics is { Count: > 0 }, () =>
        {
            RuleForEach(x => x.Topics!)
                .Cascade(CascadeMode.Stop)
                .Must(topic => !string.IsNullOrWhiteSpace(topic))
                .WithMessage("Topic filters cannot be empty.")
                .Must(topic => topic.Trim().Length <= 120)
                .WithMessage("Topic filters must be 120 characters or fewer.");
        });
        RuleFor(x => x.SourceIds)
            .Must(s => s == null || s.Count <= 30)
            .WithMessage("At most 30 sources may be used for filtering.");
        When(x => x.SourceIds is { Count: > 0 }, () =>
        {
            RuleForEach(x => x.SourceIds!)
                .GreaterThan(0)
                .WithMessage("Source filters must be positive integers.");
        });
    }
}
