using PositiveNews.Domain.Exceptions;

namespace PositiveNews.Domain.Entities;

public class Comment
{
    private readonly List<Comment> _replies = [];

    // For EF Core materialization
    private Comment() { }

    public long Id { get; private set; }
    public long ArticleId { get; private set; }
    public long UserId { get; private set; }
    public long? ParentId { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime? EditedAt { get; private set; }
    public bool IsActive { get; private set; } = true;
    public long? ModeratedBy { get; private set; }

    // Navigation
    public ArticleMetadata Article { get; private set; } = null!;
    public User User { get; private set; } = null!;
    public Comment? Parent { get; private set; }
    public User? Moderator { get; private set; }
    public IReadOnlyCollection<Comment> Replies => _replies.AsReadOnly();

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

    public void Deactivate(long moderatorId)
    {
        if (!IsActive)
            throw new DomainException("Comment is already inactive.");
        IsActive = false;
        ModeratedBy = moderatorId;
    }
}
