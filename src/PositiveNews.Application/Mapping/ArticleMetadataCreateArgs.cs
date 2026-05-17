namespace PositiveNews.Application.Mapping;

/// <summary>
/// Intermediate constructor arguments for building <see cref="PositiveNews.Domain.Entities.ArticleMetadata"/> from RSS DTOs.
/// </summary>
/// <param name="SourceId">Owning news source id.</param>
/// <param name="Title">Article title.</param>
/// <param name="Url">Canonical article URL.</param>
/// <param name="ExternalId">Optional external identifier from the feed.</param>
/// <param name="PublishedAt">Publication instant.</param>
/// <param name="LanguageCode">Language applied to new rows.</param>
/// <param name="PositivityScore">Computed positivity score from ingestion.</param>
/// <param name="Author">Optional author string.</param>
/// <param name="SummaryShort">Short summary typically sourced from description.</param>
/// <param name="ImageTag">Optional hero image markup.</param>
public sealed record ArticleMetadataCreateArgs(
    int SourceId,
    string Title,
    string Url,
    string? ExternalId,
    DateTime PublishedAt,
    string LanguageCode,
    decimal? PositivityScore,
    string? Author,
    string? SummaryShort,
    string? ImageTag);
