using PositiveNews.Application.DTOs.Admin;

namespace PositiveNews.Application.Abstractions.Persistence.Repositories.Read;

public interface IAuditLogReadRepository
{
    /// <summary>
    /// Returns recent audit log rows ordered by CreatedAt descending.
    /// </summary>
    Task<IReadOnlyList<AuditLogAdminItemDto>> GetRecentAuditLogsAsync(int limit = 100, CancellationToken cancellationToken = default);
}
