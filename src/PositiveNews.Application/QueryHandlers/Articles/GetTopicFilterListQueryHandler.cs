using MediatR;
using Microsoft.EntityFrameworkCore;
using PositiveNews.Application.Abstractions.Persistence;
using PositiveNews.Application.Queries.Articles;

namespace PositiveNews.Application.QueryHandlers.Articles;

public sealed class GetTopicFilterListQueryHandler(IIngestionDbContext db)
    : IRequestHandler<GetTopicFilterListQuery, IReadOnlyList<string>>
{
    public async Task<IReadOnlyList<string>> Handle(GetTopicFilterListQuery request, CancellationToken cancellationToken)
    {
        return await db.Topics
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .Select(t => t.Name)
            .ToListAsync(cancellationToken);
    }
}
