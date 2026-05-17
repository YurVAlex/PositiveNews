using FluentValidation;

namespace PositiveNews.Application.Queries.Articles;

/// <summary>
/// Placeholder validator with no additional constraints on <see cref="GetSourceFilterListQuery"/>.
/// </summary>
public sealed class GetSourceFilterListQueryValidator : AbstractValidator<GetSourceFilterListQuery>
{
    /// <summary>
    /// Initializes validation rules (currently none).
    /// </summary>
    public GetSourceFilterListQueryValidator()
    {
    }
}
