using Microsoft.EntityFrameworkCore;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.DTOs;
using PositiveNews.Application.DTOs.Admin;
using PositiveNews.Application.DTOs.Articles;
using PositiveNews.Application.Mapping;
using PositiveNews.Infrastructure.Persistence;

namespace PositiveNews.Infrastructure.Persistence.Repositories.Read;

/// <inheritdoc />
internal sealed class SourceReadRepository(AppDbContext db) : ISourceReadRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<IngestionSourceSnapshot>> GetActiveIngestionSourcesAsync(CancellationToken ct)
    {
        return await db.Sources
            .AsNoTracking()
            .Where(s => s.IsActive && s.FeedUrl != null)
            .OrderBy(s => s.Id)
            .ProjectToIngestionSourceSnapshot()
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<int>> GetExistingSourceIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct)
    {
        if (ids.Count == 0)
        {
            return Array.Empty<int>();
        }

        return await db.Sources
            .AsNoTracking()
            .Where(s => ids.Contains(s.Id))
            .Select(s => s.Id)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SourceFilterItemDto>> GetSourceFilterListAsync(CancellationToken ct)
    {
        return await db.Sources
            .AsNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .Select(s => new SourceFilterItemDto
            {
                Id = s.Id,
                Name = s.Name,
                LogoUrl = s.LogoUrl
            })
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SourceAdminItemDto>> GetAdminSourceListAsync(CancellationToken ct)
    {
        return await db.Sources
            .AsNoTracking()
            .OrderBy(s => s.Id)
            .Select(s => new SourceAdminItemDto
            {
                Id = s.Id,
                Name = s.Name,
                TrustScore = s.TrustScore,
                IsActive = s.IsActive,
                ModeratedBy = s.ModeratedBy
            })
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<SourceAdminDetailDto?> GetAdminSourceDetailAsync(int sourceId, CancellationToken ct)
    {
        return await db.Sources
            .AsNoTracking()
            .Where(s => s.Id == sourceId)
            .Select(s => new SourceAdminDetailDto
            {
                Id = s.Id,
                Name = s.Name,
                TrustScore = s.TrustScore,
                IsActive = s.IsActive,
                FeedUrl = s.FeedUrl ?? string.Empty,
                ModeratedBy = s.ModeratedBy
            })
            .SingleOrDefaultAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SourceFilterItemDto>> GetSourceFilterItemsByIdsAsync(
        IReadOnlyList<int> ids,
        CancellationToken ct)
    {
        if (ids.Count == 0)
        {
            return Array.Empty<SourceFilterItemDto>();
        }

        var rows = await db.Sources
            .AsNoTracking()
            .Where(s => ids.Contains(s.Id))
            .Select(s => new SourceFilterItemDto
            {
                Id = s.Id,
                Name = s.Name,
                LogoUrl = s.LogoUrl
            })
            .ToListAsync(ct);

        var byId = rows.ToDictionary(s => s.Id);
        return ids
            .Where(byId.ContainsKey)
            .Select(id => byId[id])
            .ToList();
    }
}
