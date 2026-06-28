namespace PositiveNews.Application.DTOs.Admin;

/// <summary>
/// Comment row for admin list views.
/// </summary>
public sealed class CommentAdminItemDto
{
    /// <summary>Comment primary key.</summary>
    public long Id { get; init; }

    /// <summary>Article the comment belongs to.</summary>
    public long ArticleId { get; init; }

    /// <summary>Author user id.</summary>
    public long UserId { get; init; }

    /// <summary>Number of complaints filed against this comment.</summary>
    public int ComplaintCount { get; init; }

    /// <summary>Whether the comment is visible to readers.</summary>
    public bool IsActive { get; init; }

    /// <summary>Moderator who last changed comment state, if any.</summary>
    public long? ModeratedBy { get; init; }
}
