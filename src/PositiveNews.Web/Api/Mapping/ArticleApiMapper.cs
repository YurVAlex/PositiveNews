using PositiveNews.Application.DTOs.Articles;
using PositiveNews.Web.Api.Models;
using Riok.Mapperly.Abstractions;

namespace PositiveNews.Web.Api.Mapping;

[Mapper]
public static partial class ArticleApiMapper
{
    public static partial ArticlePreviewResponse ToArticlePreviewResponse(this ArticleFeedItemDto source);

    public static partial ArticleDetailResponse ToArticleDetailResponse(this ArticleDetailDto source);

    [MapProperty(nameof(ArticleFeedPageResult.Articles), nameof(ArticleFeedResponse.Articles))]
    public static partial ArticleFeedResponse ToArticleFeedResponse(this ArticleFeedPageResult source);
}
