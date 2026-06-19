using PositiveNews.Domain.Exceptions;

namespace PositiveNews.Domain.Entities;

/// <summary>
/// User-authored comment on an article, optionally threaded under a parent comment.
/// </summary>
public class Comment
{
    private readonly List<Comment> _replies = [];
    private readonly List<Complaint> _complaints = [];

    /// <remarks>Used by EF Core when hydrating entities from the database.</remarks>
    private Comment() { }

    /// <summary>Primary key.</summary>
    public long Id { get; private set; }

    /// <summary>Article this comment belongs to.</summary>
    public long ArticleId { get; private set; }

    /// <summary>Author user id.</summary>
    public long UserId { get; private set; }

    /// <summary>Parent comment id when this is a reply; null for top-level comments.</summary>
    public long? ParentId { get; private set; }

    /// <summary>Comment body text.</summary>
    public string Content { get; private set; } = string.Empty;

    /// <summary>UTC creation time.</summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>Set when the body was last edited.</summary>
    public DateTime? EditedAt { get; private set; }

    /// <summary>When false, the comment is hidden from readers.</summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>Moderator who deactivated the comment, if any.</summary>
    public long? ModeratedBy { get; private set; }

    /// <summary>Owning article.</summary>
    public ArticleMetadata Article { get; private set; } = null!;

    /// <summary>Author.</summary>
    public User User { get; private set; } = null!;

    /// <summary>Parent comment when <see cref="ParentId"/> is set.</summary>
    public Comment? Parent { get; private set; }

    /// <summary>Moderator navigation when moderated.</summary>
    public User? Moderator { get; private set; }

    /// <summary>Direct replies (nested comments).</summary>
    public IReadOnlyCollection<Comment> Replies => _replies.AsReadOnly();

    /// <summary>Complaints filed against this comment.</summary>
    public IReadOnlyCollection<Complaint> Complaints => _complaints.AsReadOnly();

    /// <summary>
    /// Creates a new comment with trimmed content and optional parent reply id.
    /// </summary>
    public static Comment Create(long articleId, long userId, string content, long? parentId = null)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new DomainException("Comment content cannot be empty.");
        if (content.Length > 2000)
            throw new DomainException("Comment content cannot exceed 2000 characters.");

        return new Comment
        {
            ArticleId = articleId,
            UserId = userId,
            ParentId = parentId,
            Content = content.Trim(),
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
    }

    /// <summary>
    /// Updates the body and sets <see cref="EditedAt"/>; fails if inactive.
    /// </summary>
    public void Edit(string newContent)
    {
        if (string.IsNullOrWhiteSpace(newContent))
            throw new DomainException("Comment content cannot be empty.");
        if (newContent.Length > 2000)
            throw new DomainException("Comment content cannot exceed 2000 characters.");
        if (!IsActive)
            throw new DomainException("Cannot edit an inactive comment.");

        Content = newContent.Trim();
        EditedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Soft-deletes the comment and records the moderator.
    /// </summary>
    public void Deactivate(long moderatorId)
    {
        if (!IsActive)
            throw new DomainException("Comment is already inactive.");
        IsActive = false;
        ModeratedBy = moderatorId;
    }
}
