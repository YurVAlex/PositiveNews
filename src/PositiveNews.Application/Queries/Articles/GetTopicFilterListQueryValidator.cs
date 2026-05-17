using FluentValidation;

namespace PositiveNews.Application.Queries.Articles;

/// <summary>
/// Placeholder validator with no additional constraints on <see cref="GetTopicFilterListQuery"/>.
/// </summary>
public sealed class GetTopicFilterListQueryValidator : AbstractValidator<GetTopicFilterListQuery>
{
    /// <summary>
    /// Initializes validation rules (currently none).
    /// </summary>
    public GetTopicFilterListQueryValidator()
    {
    }
}
