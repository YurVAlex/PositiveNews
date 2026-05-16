namespace PositiveNews.Web.Api.Models;

/// <summary>
/// Query string payload for retrieving a page from the article feed.
/// </summary>
public sealed class GetArticleFeedRequest
{
    /// <summary>
    /// Gets the one-based page index.
    /// </summary>
    public int Page { get; init; } = 1;

    /// <summary>
    /// Gets optional topic filters (repeatable query string parameter).
    /// </summary>
    public string[]? Topic { get; init; }

    /// <summary>
    /// Gets optional preferred source ids (repeatable query string parameter).
    /// </summary>
    public int[]? Source { get; init; }

    /// <summary>
    /// Gets optional sort mode (for example: <c>positivity</c>).
    /// </summary>
    public string? Sort { get; init; }
}
