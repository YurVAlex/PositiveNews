using FluentValidation;

namespace PositiveNews.Application.Queries.Ingestion;

public sealed class GetTopicLookupQueryValidator : AbstractValidator<GetTopicLookupQuery>
{
    public GetTopicLookupQueryValidator()
    {
    }
}
