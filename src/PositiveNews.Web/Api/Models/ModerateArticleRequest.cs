namespace PositiveNews.Web.Api.Models;

/// <summary>
/// Request payload for article moderation actions.
/// </summary>
public sealed class ModerateArticleRequest
{
    public bool IsActive { get; init; }
    public string? Reason { get; init; }
    public string? Note { get; init; }
}
