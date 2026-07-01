using PositiveNews.Application.DTOs.Ingestion;
using PositiveNews.Application.DTOs.Articles;
using PositiveNews.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace PositiveNews.Application.Mapping;

/// <summary>
/// Mapperly projections between persistence entities and article DTOs plus RSS ingestion factories.
/// </summary>
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class ArticleMapper
{
    /// <summary>
    /// Projects an article aggregate with joined source and topics into a feed card DTO.
    /// </summary>
    [MapProperty("Source.Name", nameof(ArticleFeedItemDto.SourceName))]
    [MapProperty("Source.LogoUrl", nameof(ArticleFeedItemDto.SourceLogoUrl))]
    [MapProperty("Source.TrustScore", nameof(ArticleFeedItemDto.SourceTrustScore))]
    [MapProperty("ArticleTopics", nameof(ArticleFeedItemDto.Topics))]
    public static partial ArticleFeedItemDto ToArticleFeedItemDto(this ArticleMetadata source);

    /// <summary>
    /// Projects an article aggregate with source and content into a detail DTO.
    /// </summary>
    [MapProperty("Source.Name", nameof(ArticleDetailDto.SourceName))]
    [MapProperty("Source.LogoUrl", nameof(ArticleDetailDto.SourceLogoUrl))]
    [MapProperty("Content.ContentRaw", nameof(ArticleDetailDto.ContentHtml))]
    public static partial ArticleDetailDto ToArticleDetailDto(this ArticleMetadata source);

    /// <summary>
    /// EF-safe projection for feed queries.
    /// </summary>
    public static partial IQueryable<ArticleFeedItemDto> ProjectToArticleFeedItemDto(this IQueryable<ArticleMetadata> query);

    /// <summary>
    /// Maps RSS ingestion DTO fields into arguments for <see cref="ArticleMetadata.Create"/>.
    /// </summary>
    /// <param name="source">Parsed feed item.</param>
    /// <param name="sourceId">Database source identifier.</param>
    /// <param name="defaultLanguageCode">Language applied to stored metadata.</param>
    /// <returns>Arguments consumed by the persistence handler.</returns>
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

    /// <summary>
    /// Maps joined topic navigation properties to display strings for feed cards.
    /// </summary>
    private static string MapArticleTopicToString(ArticleTopic articleTopic)
    {
        return articleTopic.Topic.Name;
    }
}
