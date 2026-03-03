namespace PositiveNews.Domain.Entities;

public class Comment
{
    public long Id { get; set; }
    public long ArticleId { get; set; }
    public long UserId { get; set; }
    public long? ParentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? EditedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public long? ModeratedBy { get; set; }

    // Navigation
    public ArticleMetadata Article { get; set; } = null!;
    public User User { get; set; } = null!;
    public Comment? Parent { get; set; }
    public User? Moderator { get; set; }
    public ICollection<Comment> Replies { get; set; } = new List<Comment>();
}