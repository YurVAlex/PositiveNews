using FluentValidation;

namespace PositiveNews.Application.Queries.Articles;

public sealed class GetArticleDetailQueryValidator : AbstractValidator<GetArticleDetailQuery>
{
    public GetArticleDetailQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
