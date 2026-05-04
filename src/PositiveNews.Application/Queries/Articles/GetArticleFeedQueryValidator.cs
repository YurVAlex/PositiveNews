using FluentValidation;
using PositiveNews.Application.Abstractions.Persistence.Models;

namespace PositiveNews.Application.Queries.Articles;

public sealed class GetArticleFeedQueryValidator : AbstractValidator<GetArticleFeedQuery>
{
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
