using PositiveNews.Application.DTOs;
using PositiveNews.Application.DTOs.Articles;
using PositiveNews.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace PositiveNews.Application.Mapping;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class ArticleMapper
{
    [MapProperty("Source.Name", nameof(ArticleFeedItemDto.SourceName))]
    [MapProperty("Source.LogoUrl", nameof(ArticleFeedItemDto.SourceLogoUrl))]
    [MapProperty("ArticleTopics", nameof(ArticleFeedItemDto.Topics))]
    public static partial ArticleFeedItemDto ToArticleFeedItemDto(this ArticleMetadata source);

    [MapProperty("Source.Name", nameof(ArticleDetailDto.SourceName))]
    [MapProperty("Source.LogoUrl", nameof(ArticleDetailDto.SourceLogoUrl))]
    [MapProperty("Content.ContentRaw", nameof(ArticleDetailDto.ContentHtml))]
    public static partial ArticleDetailDto ToArticleDetailDto(this ArticleMetadata source);

    public static partial IQueryable<ArticleFeedItemDto> ProjectToArticleFeedItemDto(this IQueryable<ArticleMetadata> query);

    public static ArticleMetadataCreateArgs ToArticleMetadataCreateArgs(
        this RssFeedItemDto source,
        int sourceId,
        string defaultLanguageCode)
    {
        return new ArticleMetadataCreateArgs(
            SourceId: sourceId,
            Title: source.Title,
            Url: source.Link,
            ExternalId: source.ExternalId,
            PublishedAt: source.PublishedDate,
            LanguageCode: defaultLanguageCode,
            PositivityScore: source.PositivityScore,
            Author: source.Author,
            SummaryShort: source.Description,
            ImageTag: source.ImageTag);
    }

    private static string MapArticleTopicToString(ArticleTopic articleTopic)
    {
        return articleTopic.Topic.Name;
    }
}
