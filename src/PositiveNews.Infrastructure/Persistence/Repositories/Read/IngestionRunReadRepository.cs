using Microsoft.EntityFrameworkCore;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.DTOs.Ingestion;

namespace PositiveNews.Infrastructure.Persistence.Repositories.Read;

/// <inheritdoc />
internal sealed class IngestionRunReadRepository(AppDbContext db) : IIngestionRunReadRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<IngestionRunListItemDto>> GetLatestAsync(int limit, CancellationToken ct)
    {
        var take = Math.Clamp(limit, 1, 500);

        var runs = await db.IngestionRuns
            .AsNoTracking()
            .Include(r => r.Source)
            .OrderByDescending(r => r.Id)
            .Take(take)
            .ToListAsync(ct);

        return runs
            .Select(r => new IngestionRunListItemDto
            {
                Id = r.Id,
                SourceName = r.Source.Name,
                StartedAt = r.StartedAt,
                FinishedAt = r.FinishedAt,
                Status = r.Status.ToString(),
                ItemsFetched = r.ItemsFetched
            })
            .ToList();
    }
}
