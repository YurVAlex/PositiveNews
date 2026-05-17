namespace PositiveNews.Domain.Enums;

/// <summary>
/// Kind of entity referenced by an <see cref="Entities.AuditLog"/> row.
/// </summary>
public enum AuditEntityType
{
    /// <summary>Article metadata aggregate.</summary>
    Article,

    /// <summary>Comment aggregate.</summary>
    Comment,

    /// <summary>User account.</summary>
    User,

    /// <summary>News source.</summary>
    Source
}
