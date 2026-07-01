namespace PositiveNews.Application.DTOs.Ingestion;

/// <summary>
/// Outcome of processing an entire RSS document into DTOs.
/// </summary>
/// <param name="Items">Accepted items ready for deduplication and persistence.</param>
/// <param name="InvalidCount">Number of items skipped due to validation or parse errors.</param>
public sealed record FeedProcessingResult(
    IReadOnlyList<RssFeedItemDto> Items,
    int InvalidCount);
