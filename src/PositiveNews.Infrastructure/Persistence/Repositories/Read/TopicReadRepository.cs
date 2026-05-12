using Microsoft.EntityFrameworkCore;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.DTOs;
using PositiveNews.Application.Mapping;
using PositiveNews.Infrastructure.Persistence;

namespace PositiveNews.Infrastructure.Persistence.Repositories.Read;

/// <inheritdoc />
internal sealed class TopicReadRepository(AppDbContext db) : ITopicReadRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> GetTopicNamesAsync(CancellationToken ct)
    {
        return await db.Topics
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .Select(t => t.Name)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TopicSnapshot>> GetAllTopicSnapshotsAsync(CancellationToken ct)
    {
        return await db.Topics
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .ProjectToTopicSnapshot()
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<string, int>> GetTopicIdsByNamesAsync(IReadOnlyCollection<string> names, CancellationToken ct)
    {
        if (names.Count == 0)
            return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        var rows = await db.Topics.AsNoTracking()
            .Where(t => names.Contains(t.Name))
            .Select(t => new { t.Id, t.Name })
            .ToListAsync(ct);

        return rows.ToDictionary(r => r.Name, r => r.Id, StringComparer.OrdinalIgnoreCase);
    }
}
