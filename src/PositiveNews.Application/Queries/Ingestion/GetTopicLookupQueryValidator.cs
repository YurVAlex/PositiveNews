using FluentValidation;

namespace PositiveNews.Application.Queries.Ingestion;

/// <summary>
/// Placeholder validator with no parameters on <see cref="GetTopicLookupQuery"/>.
/// </summary>
public sealed class GetTopicLookupQueryValidator : AbstractValidator<GetTopicLookupQuery>
{
    /// <summary>
    /// Initializes validation rules (currently none).
    /// </summary>
    public GetTopicLookupQueryValidator()
    {
    }
}
