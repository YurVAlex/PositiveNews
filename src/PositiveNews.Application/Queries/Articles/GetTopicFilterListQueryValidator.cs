using FluentValidation;

namespace PositiveNews.Application.Queries.Articles;

public sealed class GetTopicFilterListQueryValidator : AbstractValidator<GetTopicFilterListQuery>
{
    public GetTopicFilterListQueryValidator()
    {
    }
}
