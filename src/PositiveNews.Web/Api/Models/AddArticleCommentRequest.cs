namespace PositiveNews.Web.Api.Models;

/// <summary>
/// Request body for creating a new article comment.
/// </summary>
public sealed class AddArticleCommentRequest
{
    /// <summary>Comment body text.</summary>
    public string Content { get; init; } = string.Empty;
}
