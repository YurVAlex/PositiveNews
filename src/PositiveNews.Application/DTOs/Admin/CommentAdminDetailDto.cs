namespace PositiveNews.Application.DTOs.Admin;

/// <summary>
/// Comment detail for admin moderation.
/// </summary>
public sealed class CommentAdminDetailDto
{
    /// <summary>Comment primary key.</summary>
    public long Id { get; init; }

    /// <summary>Comment body text.</summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>UTC creation time.</summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>Author user id.</summary>
    public long UserId { get; init; }

    /// <summary>Author display name.</summary>
    public string UserName { get; init; } = string.Empty;

    /// <summary>Whether the comment is visible to readers.</summary>
    public bool IsActive { get; init; }

    /// <summary>Moderator who last changed comment state, if any.</summary>
    public long? ModeratedBy { get; init; }

    /// <summary>Article the comment belongs to.</summary>
    public long ArticleId { get; init; }

    /// <summary>Complaints filed against this comment.</summary>
    public IReadOnlyList<CommentComplaintAdminItemDto> Complaints { get; init; } = [];
}
