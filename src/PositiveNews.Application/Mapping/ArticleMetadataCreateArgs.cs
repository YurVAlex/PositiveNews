namespace PositiveNews.Application.Mapping;

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
