using PositiveNews.Domain.Enums;
using PositiveNews.Domain.Exceptions;

namespace PositiveNews.Domain.Entities;

/// <summary>
/// Records a moderator action or field-level change for auditing (articles, users, sources, etc.).
/// </summary>
public class AuditLog
{
    /// <remarks>Used by EF Core when hydrating entities from the database.</remarks>
    private AuditLog() { }

    /// <summary>Primary key.</summary>
    public long Id { get; private set; }

    /// <summary>Which aggregate type was affected.</summary>
    public AuditEntityType EntityType { get; private set; }

    /// <summary>Primary key of the affected entity.</summary>
    public long EntityId { get; private set; }

    /// <summary>Optional name of the changed field.</summary>
    public string? ChangedField { get; private set; }

    /// <summary>Previous value snapshot.</summary>
    public string? OldValue { get; private set; }

    /// <summary>New value snapshot.</summary>
    public string? NewValue { get; private set; }

    /// <summary>Optional machine-readable reason code.</summary>
    public string? Reason { get; private set; }

    /// <summary>Optional free-form note.</summary>
    public string? Note { get; private set; }

    /// <summary>Moderator who performed the action.</summary>
    public long ModeratorId { get; private set; }

    /// <summary>UTC timestamp when the log row was created.</summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>Moderator navigation.</summary>
    public User Moderator { get; private set; } = null!;

    /// <summary>
    /// Builds a new audit entry with the given entity reference and optional diff fields.
    /// </summary>
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
