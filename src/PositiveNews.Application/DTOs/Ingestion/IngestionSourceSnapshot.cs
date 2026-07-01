namespace PositiveNews.Application.DTOs.Ingestion;

/// <summary>
/// Immutable view of a source row needed for one ingestion pass (no EF tracking).
/// </summary>
/// <param name="Id">Source primary key.</param>
/// <param name="Name">Human-readable source name.</param>
/// <param name="FeedUrl">RSS feed URL.</param>
/// <param name="DefaultLanguageCode">BCP 47 or internal language code for new articles.</param>
/// <param name="DefaultThumbnailHtml">Optional fallback image markup when feeds omit images.</param>
public sealed record IngestionSourceSnapshot(
    int Id, string Name, string FeedUrl, string DefaultLanguageCode, string? DefaultThumbnailHtml);
