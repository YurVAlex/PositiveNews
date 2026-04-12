using MediatR;
using Microsoft.EntityFrameworkCore;
using PositiveNews.Application.Abstractions.Persistence;
using PositiveNews.Application.DTOs;
using PositiveNews.Application.Ingestion;
using PositiveNews.Application.Queries.Ingestion;

namespace PositiveNews.Application.QueryHandlers.Ingestion;

public sealed class FindExistingArticleKeysQueryHandler(IIngestionDbContext db)
    : IRequestHandler<FindExistingArticleKeysQuery, ExistingArticleKeys>
{
    public async Task<ExistingArticleKeys> Handle(
        FindExistingArticleKeysQuery request,
        CancellationToken cancellationToken)
    {
        var result = new ExistingArticleKeys();

        var extDistinct = request.ExternalIds
            .Where(x => !string.IsNullOrEmpty(x))
            .Distinct()
            .ToList();

        foreach (var chunk in extDistinct.Chunk(IngestionPipelineConstants.SqlInClauseChunkSize))
        {
            var chunkArr = chunk.ToArray();
            var batch = await db.ArticlesMetadata.AsNoTracking()
                .Where(a => a.ExternalId != null && chunkArr.Contains(a.ExternalId))
                .Select(a => a.ExternalId!)
                .ToListAsync(cancellationToken);
            foreach (var id in batch)
                result.ExternalIds.Add(id);
        }

        var urls = request.Urls.Distinct().ToList();
        foreach (var chunk in urls.Chunk(IngestionPipelineConstants.SqlInClauseChunkSize))
        {
            var chunkArr = chunk.ToArray();
            var batch = await db.ArticlesMetadata.AsNoTracking()
                .Where(a => chunkArr.Contains(a.Url))
                .Select(a => a.Url)
                .ToListAsync(cancellationToken);
            foreach (var u in batch)
                result.Urls.Add(u);
        }

        var titles = request.Titles.Distinct().ToList();
        foreach (var chunk in titles.Chunk(IngestionPipelineConstants.SqlInClauseChunkSize))
        {
            var chunkArr = chunk.ToArray();
            var batch = await db.ArticlesMetadata.AsNoTracking()
                .Where(a => chunkArr.Contains(a.Title))
                .Select(a => a.Title)
                .ToListAsync(cancellationToken);
            foreach (var t in batch)
                result.Titles.Add(t);
        }

        return result;
    }
}
