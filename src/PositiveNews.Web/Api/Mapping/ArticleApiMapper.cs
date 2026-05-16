using PositiveNews.Application.Abstractions.Persistence.Models;
using PositiveNews.Application.DTOs.Articles;
using PositiveNews.Application.Queries.Articles;
using PositiveNews.Web.Api.Models;
using Riok.Mapperly.Abstractions;

namespace PositiveNews.Web.Api.Mapping;

/// <summary>
/// Mapperly mappings between article DTOs and HTTP API response models.
/// </summary>
[Mapper]
public static partial class ArticleApiMapper
{
    /// <summary>
    /// Maps a feed row to an <see cref="Models.ArticlePreviewResponse"/>.
    /// </summary>
    /// <param name="source">Application article feed item.</param>
    /// <returns>The preview DTO for API responses.</returns>
    public static partial ArticlePreviewResponse ToArticlePreviewResponse(this ArticleFeedItemDto source);

    /// <summary>
    /// Maps article detail data to an <see cref="Models.ArticleDetailResponse"/>.
    /// </summary>
    /// <param name="source">Application article detail DTO.</param>
    /// <returns>The detail DTO for API responses.</returns>
    public static partial ArticleDetailResponse ToArticleDetailResponse(this ArticleDetailDto source);

    /// <summary>
    /// Maps a paged feed result including articles and paging metadata.
    /// </summary>
    /// <param name="source">Paged feed result from the application layer.</param>
    /// <returns>The wire-format feed response.</returns>
    /// <remarks>
    /// Explicit property mapping aligns source and destination article collection names.
    /// </remarks>
    [MapProperty(nameof(ArticleFeedPageResult.Articles), nameof(ArticleFeedResponse.Articles))]
    public static partial ArticleFeedResponse ToArticleFeedResponse(this ArticleFeedPageResult source);

    /// <summary>
    /// Maps query-string feed request data into the corresponding MediatR query.
    /// </summary>
    /// <param name="source">Inbound HTTP request model.</param>
    /// <returns>Application-layer feed query.</returns>
    public static GetArticleFeedQuery ToGetArticleFeedQuery(this GetArticleFeedRequest source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new GetArticleFeedQuery(
            source.Page,
            source.Topic ?? Array.Empty<string>(),
            source.Source ?? Array.Empty<int>(),
            SortBy: MapSort(source.Sort));
    }

    /// <summary>
    /// Maps API sort text to feed sort enum values and preserves invalid input for validation.
    /// </summary>
    /// <param name="sort">Raw sort query value.</param>
    /// <returns>Mapped sort value, or an undefined enum member when the value is unsupported.</returns>
    private static ArticleFeedSortBy MapSort(string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            return ArticleFeedSortBy.PublishedAt;
        }

        if (string.Equals(sort, "positivity", StringComparison.OrdinalIgnoreCase))
        {
            return ArticleFeedSortBy.PositivityScore;
        }

        if (string.Equals(sort, "preferences", StringComparison.OrdinalIgnoreCase))
        {
            return ArticleFeedSortBy.Preferences;
        }

        return Enum.TryParse<ArticleFeedSortBy>(sort, ignoreCase: true, out var parsedSort)
            ? parsedSort
            : (ArticleFeedSortBy)(-1);
    }
}
