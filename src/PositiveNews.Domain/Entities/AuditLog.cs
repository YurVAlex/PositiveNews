using PositiveNews.Domain.Enums;
using PositiveNews.Domain.Exceptions;

namespace PositiveNews.Domain.Entities;

public class AuditLog
{
    // For EF Core materialization
    private AuditLog() { }

    public long Id { get; private set; }
    public AuditEntityType EntityType { get; private set; }
    public long EntityId { get; private set; }
    public string? ChangedField { get; private set; }
    public string? OldValue { get; private set; }
    public string? NewValue { get; private set; }
    public string? Reason { get; private set; }
    public string? Note { get; private set; }
    public long ModeratorId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation
    public User Moderator { get; private set; } = null!;

    public static AuditLog Create(
        AuditEntityType entityType,
        long entityId,
        long moderatorId,
        string? changedField = null,
        string? oldValue = null,
        string? newValue = null,
        string? reason = null,
        string? note = null)
    {
        if (moderatorId <= 0)
            throw new DomainException("ModeratorId must be a valid user identifier.");

        return new AuditLog
        {
            EntityType = entityType,
            EntityId = entityId,
            ModeratorId = moderatorId,
            ChangedField = changedField,
            OldValue = oldValue,
            NewValue = newValue,
            Reason = reason,
            Note = note,
            CreatedAt = DateTime.UtcNow
        };
    }
}
