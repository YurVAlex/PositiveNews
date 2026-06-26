namespace PositiveNews.Application.DTOs.Comments;

/// <summary>
/// Minimal comment data used for complaint validation.
/// </summary>
public sealed class ActiveCommentDto
{
    /// <summary>Comment primary key.</summary>
    public long Id { get; init; }

    /// <summary>Author user id.</summary>
    public long UserId { get; init; }

    /// <summary>Article the comment belongs to.</summary>
    public long ArticleId { get; init; }
}
