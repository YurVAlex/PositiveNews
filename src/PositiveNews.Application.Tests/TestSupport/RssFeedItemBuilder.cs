using PositiveNews.Application.DTOs;

namespace PositiveNews.Application.Tests.TestSupport;

internal static class RssFeedItemBuilder
{
    public static RssFeedItemDto Create(
        string title = "Headline",
        string link = "https://news.example.com/article",
        string? externalId = "ext-1",
        DateTime? published = null,
        IReadOnlyList<string>? topics = null)
        => new()
        {
            Title = title,
            Link = link,
            ExternalId = externalId,
            PublishedDate = published ?? new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            ContentRaw = "<p>body</p>",
            Description = "Summary",
            Topics = topics ?? [],
            PositivityScore = 0.5m
        };
}
