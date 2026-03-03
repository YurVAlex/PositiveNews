using PositiveNews.Domain.Enums;

namespace PositiveNews.Domain.Entities;

public class AuditLog
{
    public long Id { get; set; }
    public AuditEntityType EntityType { get; set; }
    public long EntityId { get; set; }
    public string? ChangedField { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? Reason { get; set; }
    public string? Note { get; set; }
    public long ModeratorId { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public User Moderator { get; set; } = null!;
}