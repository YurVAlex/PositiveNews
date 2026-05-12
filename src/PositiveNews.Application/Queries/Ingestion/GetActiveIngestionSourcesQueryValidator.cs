using FluentValidation;

namespace PositiveNews.Application.Queries.Ingestion;

/// <summary>
/// Placeholder validator with no fields to validate on <see cref="GetActiveIngestionSourcesQuery"/>.
/// </summary>
public sealed class GetActiveIngestionSourcesQueryValidator : AbstractValidator<GetActiveIngestionSourcesQuery>
{
    /// <summary>
    /// Initializes validation rules (currently none).
    /// </summary>
    public GetActiveIngestionSourcesQueryValidator()
    {
    }
}
