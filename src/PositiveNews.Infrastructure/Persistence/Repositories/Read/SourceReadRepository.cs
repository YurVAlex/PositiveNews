using Microsoft.EntityFrameworkCore;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.DTOs;
using PositiveNews.Infrastructure.Persistence;

namespace PositiveNews.Infrastructure.Persistence.Repositories.Read;

internal sealed class SourceReadRepository(AppDbContext db) : ISourceReadRepository
{
    public async Task<IReadOnlyList<IngestionSourceSnapshot>> GetActiveIngestionSourcesAsync(CancellationToken ct)
    {
        return await db.Sources
            .AsNoTracking()
            .Where(s => s.IsActive && s.FeedUrl != null)
            .OrderBy(s => s.Id)
            .Select(s => new IngestionSourceSnapshot(s.Id, s.Name, s.FeedUrl!, s.DefaultLanguageCode))
            .ToListAsync(ct);
    }
}
