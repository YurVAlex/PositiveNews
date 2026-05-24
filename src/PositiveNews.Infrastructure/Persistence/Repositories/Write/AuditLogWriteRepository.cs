using PositiveNews.Application.Abstractions.Persistence.Repositories.Write;
using PositiveNews.Domain.Entities;
using PositiveNews.Infrastructure.Persistence;

namespace PositiveNews.Infrastructure.Persistence.Repositories.Write;

/// <inheritdoc />
internal sealed class AuditLogWriteRepository(AppDbContext db) : IAuditLogWriteRepository
{
    /// <inheritdoc />
    public void Add(AuditLog auditLog) => db.AuditLogs.Add(auditLog);
}
