using FluentValidation;

namespace PositiveNews.Application.Queries.Articles;

public sealed class GetArticleFeedQueryValidator : AbstractValidator<GetArticleFeedQuery>
{
    public GetArticleFeedQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
