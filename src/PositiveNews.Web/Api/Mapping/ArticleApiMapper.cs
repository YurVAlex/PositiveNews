using PositiveNews.Application.DTOs.Articles;
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
}
