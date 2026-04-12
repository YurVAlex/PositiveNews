using MediatR;
using Microsoft.EntityFrameworkCore;
using PositiveNews.Application.Abstractions.Persistence;
using PositiveNews.Application.DTOs;
using PositiveNews.Application.Queries.Ingestion;

namespace PositiveNews.Application.QueryHandlers.Ingestion;

public sealed class GetActiveIngestionSourcesQueryHandler(IIngestionDbContext db)
    : IRequestHandler<GetActiveIngestionSourcesQuery, IReadOnlyList<IngestionSourceSnapshot>>
{
    public async Task<IReadOnlyList<IngestionSourceSnapshot>> Handle(
        GetActiveIngestionSourcesQuery request,
        CancellationToken cancellationToken)
    {
        return await db.Sources
            .AsNoTracking()
            .Where(s => s.IsActive && s.FeedUrl != null)
            .OrderBy(s => s.Id)
            .Select(s => new IngestionSourceSnapshot(s.Id, s.Name, s.FeedUrl!, s.DefaultLanguageCode))
            .ToListAsync(cancellationToken);
    }
}
