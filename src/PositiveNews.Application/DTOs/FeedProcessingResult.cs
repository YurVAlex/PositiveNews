namespace PositiveNews.Application.DTOs;

public sealed record FeedProcessingResult(
    IReadOnlyList<RssFeedItemDto> Items,
    int InvalidCount);
