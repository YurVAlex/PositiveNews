using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PositiveNews.Application.Abstractions.Persistence;
using PositiveNews.Application.Commands.Ingestion;
using PositiveNews.Application.DTOs;
using PositiveNews.Application.Ingestion;
using PositiveNews.Domain.Entities;

namespace PositiveNews.Application.CommandHandlers.Ingestion;

public sealed class PersistIngestedArticlesCommandHandler(
    IIngestionDbContext db,
    ILogger<PersistIngestedArticlesCommandHandler> logger)
    : IRequestHandler<PersistIngestedArticlesCommand, int>
{
    public async Task<int> Handle(PersistIngestedArticlesCommand request, CancellationToken cancellationToken)
    {
        if (request.Items.Count == 0)
            return 0;

        var totalSaved = 0;
        var chunkSize = IngestionPipelineConstants.ArticlePersistChunkSize;

        for (var i = 0; i < request.Items.Count; i += chunkSize)
        {
            var chunk = request.Items.Skip(i).Take(chunkSize).ToList();
            var pairs = new List<(ArticleMetadata Meta, RssFeedItemDto Dto)>();

            foreach (var dto in chunk)
            {
                try
                {
                    var meta = CreateArticleMetadata(request.SourceId, request.DefaultLanguageCode, dto);
                    pairs.Add((meta, dto));
                    db.ArticlesMetadata.Add(meta);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogError(ex, "Error building article entity for external id: {ExternalId}", dto.ExternalId);
                }
            }

            if (pairs.Count == 0)
                continue;

            await db.SaveChangesAsync(cancellationToken);
            await AppendArticleTopicsAsync(pairs, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);

            foreach (var (_, dto) in pairs)
                logger.LogInformation("Ingested new article: {Title}", dto.Title);

            totalSaved += pairs.Count;
        }

        return totalSaved;
    }

    private static ArticleMetadata CreateArticleMetadata(int sourceId, string defaultLanguageCode, RssFeedItemDto dto)
    {
        var articleMeta = new ArticleMetadata
        {
            SourceId = sourceId,
            ExternalId = dto.ExternalId,
            Title = dto.Title,
            Author = dto.Author,
            Url = dto.Link,
            ImageTag = dto.ImageTag,
            PublishedAt = dto.PublishedDate,
            IngestedAt = DateTime.UtcNow,
            LanguageCode = defaultLanguageCode,
            RegionCode = "Global",
            IsActive = true,
            SummaryShort = dto.Description,
            PositivityScore = dto.PositivityScore,
            AnalyzedAt = dto.PositivityScore.HasValue ? DateTime.UtcNow : null
        };

        articleMeta.Content = new ArticleContent
        {
            ContentRaw = dto.ContentRaw,
            ContentClean = dto.ContentClean
        };

        return articleMeta;
    }

    private async Task AppendArticleTopicsAsync(
        IReadOnlyList<(ArticleMetadata Meta, RssFeedItemDto Dto)> pairs,
        CancellationToken cancellationToken)
    {
        var names = pairs
            .SelectMany(p => p.Dto.Topics ?? Enumerable.Empty<string>())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (names.Count == 0)
            return;

        var topicIdByName = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var nameChunk in names.Chunk(IngestionPipelineConstants.SqlInClauseChunkSize))
        {
            var arr = nameChunk.ToArray();
            var rows = await db.Topics
                .AsNoTracking()
                .Where(t => arr.Contains(t.Name))
                .Select(t => new { t.Id, t.Name })
                .ToListAsync(cancellationToken);

            foreach (var row in rows)
                topicIdByName[row.Name] = row.Id;
        }

        foreach (var (meta, dto) in pairs)
        {
            if (dto.Topics == null || dto.Topics.Count == 0)
                continue;

            var seenTopicIds = new HashSet<int>();
            foreach (var topicName in dto.Topics)
            {
                if (!topicIdByName.TryGetValue(topicName, out var topicId) || !seenTopicIds.Add(topicId))
                    continue;

                db.ArticleTopics.Add(new ArticleTopic
                {
                    ArticleId = meta.Id,
                    TopicId = topicId
                });
            }
        }
    }
}