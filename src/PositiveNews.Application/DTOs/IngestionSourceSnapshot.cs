namespace PositiveNews.Application.DTOs;

/// <summary>
/// Immutable view of a source row needed for one ingestion pass (no EF tracking).
/// </summary>
public sealed record IngestionSourceSnapshot(int Id, string Name, string FeedUrl, string DefaultLanguageCode);
