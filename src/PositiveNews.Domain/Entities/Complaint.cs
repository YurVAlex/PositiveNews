using PositiveNews.Domain.Exceptions;

namespace PositiveNews.Domain.Entities;

/// <summary>
/// User-submitted complaint about a comment on an article.
/// </summary>
public class Complaint
{
    /// <remarks>Used by EF Core when hydrating entities from the database.</remarks>
    private Complaint() { }

    /// <summary>Primary key.</summary>
    public long Id { get; private set; }

    /// <summary>User who filed the complaint.</summary>
    public long UserId { get; private set; }

    /// <summary>Comment being complained about.</summary>
    public long CommentId { get; private set; }

    /// <summary>UTC time the complaint was filed.</summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>Complaint reason text.</summary>
    public string Reason { get; private set; } = string.Empty;

    /// <summary>Complainant navigation.</summary>
    public User User { get; private set; } = null!;

    /// <summary>Comment navigation.</summary>
    public Comment Comment { get; private set; } = null!;

    /// <summary>
    /// Creates a new complaint with trimmed reason text.
    /// </summary>
    public static Complaint Create(long userId, long commentId, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Complaint reason cannot be empty.");
        if (reason.Length > 500)
            throw new DomainException("Complaint reason cannot exceed 500 characters.");

        return new Complaint
        {
            UserId = userId,
            CommentId = commentId,
            Reason = reason.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }
}
