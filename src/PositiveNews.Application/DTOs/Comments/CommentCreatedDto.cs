namespace PositiveNews.Application.DTOs.Comments;

/// <summary>
/// Newly created comment returned after a successful POST.
/// </summary>
public sealed class CommentCreatedDto
{
    /// <summary>Comment primary key.</summary>
    public long Id { get; init; }

    /// <summary>Author user id.</summary>
    public long UserId { get; init; }

    /// <summary>Author display name.</summary>
    public string UserName { get; init; } = string.Empty;

    /// <summary>Comment body text.</summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>UTC creation time.</summary>
    public DateTime CreatedAt { get; init; }
}
