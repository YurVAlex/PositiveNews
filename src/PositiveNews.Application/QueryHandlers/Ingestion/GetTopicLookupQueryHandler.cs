using MediatR;
using Microsoft.EntityFrameworkCore;
using PositiveNews.Application.Abstractions.Persistence;
using PositiveNews.Application.DTOs;
using PositiveNews.Application.Queries.Ingestion;

namespace PositiveNews.Application.QueryHandlers.Ingestion;

public sealed class GetTopicLookupQueryHandler(IIngestionDbContext db)
    : IRequestHandler<GetTopicLookupQuery, TopicLookup>
{
    public async Task<TopicLookup> Handle(GetTopicLookupQuery request, CancellationToken cancellationToken)
    {
        var topics = await db.Topics.AsNoTracking().ToListAsync(cancellationToken);
        return TopicLookup.Build(topics);
    }
}
