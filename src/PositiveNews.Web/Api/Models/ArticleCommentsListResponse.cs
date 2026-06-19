namespace PositiveNews.Web.Api.Models;

/// <summary>
/// List of comments for an article.
/// </summary>
public sealed class ArticleCommentsListResponse
{
    /// <summary>Active top-level comments ordered by creation time.</summary>
    public IReadOnlyList<CommentResponse> Comments { get; init; } = [];
}
