using FluentValidation;

namespace PositiveNews.Application.Queries.Ingestion;

public sealed class GetActiveIngestionSourcesQueryValidator : AbstractValidator<GetActiveIngestionSourcesQuery>
{
    public GetActiveIngestionSourcesQueryValidator()
    {
    }
}
