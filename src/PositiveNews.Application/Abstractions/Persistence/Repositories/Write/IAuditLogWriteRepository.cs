using PositiveNews.Domain.Entities;

namespace PositiveNews.Application.Abstractions.Persistence.Repositories.Write;

/// <summary>
/// Writes audit log records recorded after admin or moderation actions.
/// </summary>
public interface IAuditLogWriteRepository
{
    /// <summary>
    /// Stages an audit log row for insertion on commit.
    /// </summary>
    /// <param name="auditLog">The audit log entry.</param>
    void Add(AuditLog auditLog);
}
