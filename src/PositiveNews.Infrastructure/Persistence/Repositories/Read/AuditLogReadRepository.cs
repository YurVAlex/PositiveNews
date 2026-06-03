using Microsoft.EntityFrameworkCore;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.DTOs.Admin;
using PositiveNews.Domain.Entities;
using PositiveNews.Infrastructure.Persistence;

namespace PositiveNews.Infrastructure.Persistence.Repositories.Read;

internal sealed class AuditLogReadRepository(AppDbContext db) : IAuditLogReadRepository
{
    public async Task<IReadOnlyList<AuditLogAdminItemDto>> GetRecentAuditLogsAsync(int limit = 100, CancellationToken cancellationToken = default)
    {
        var query = db.AuditLogs
            .AsNoTracking()
            .OrderByDescending(al => al.CreatedAt)
            .Take(limit)
            .Select(al => new AuditLogAdminItemDto
            {
                Id = al.Id,
                EntityType = al.EntityType,
                EntityId = al.EntityId,
                ChangedField = al.ChangedField,
                OldValue = al.OldValue,
                NewValue = al.NewValue,
                Reason = al.Reason,
                Note = al.Note,
                CreatedAt = al.CreatedAt,
                ModeratorId = al.ModeratorId
            });

        return await query.ToListAsync(cancellationToken);
    }
}
