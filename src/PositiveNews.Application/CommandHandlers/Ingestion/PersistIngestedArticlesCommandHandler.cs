using MediatR;
using Microsoft.Extensions.Logging;
using PositiveNews.Application.Abstractions.Persistence;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Write;
using PositiveNews.Application.Common;
using PositiveNews.Application.Commands.Ingestion;
using PositiveNews.Application.DTOs;
using PositiveNews.Application.Ingestion;
using PositiveNews.Application.Mapping;
using PositiveNews.Domain.Entities;
using PositiveNews.Domain.Exceptions;

namespace PositiveNews.Application.CommandHandlers.Ingestion;

/// <summary>
/// Maps RSS DTOs to domain entities in bounded chunks, resolves topic IDs, and persists via the ingestion unit of work.
/// </summary>
/// <param name="articleWriteRepository">Stages article aggregates.</param>
/// <param name="ingestionArticleBatchSaver">Persists staged articles with duplicate fallback.</param>
/// <param name="logger">Structured logging for successes and domain violations.</param>
public sealed class PersistIngestedArticlesCommandHandler(
    IArticleWriteRepository articleWriteRepository,
    IIngestionArticleBatchSaver ingestionArticleBatchSaver,
    ILogger<PersistIngestedArticlesCommandHandler> logger)
    : IRequestHandler<PersistIngestedArticlesCommand, Result<int>>
{
    /// <summary>
    /// Builds <see cref="ArticleMetadata"/> rows from DTOs, attaches content and topics, saves in chunks, and returns count saved.
    /// </summary>
    /// <param name="request">Source id, language code, and items to persist.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of articles persisted or a failure when domain rules are violated.</returns>
    public async Task<Result<int>> Handle(PersistIngestedArticlesCommand request, CancellationToken cancellationToken)
    {
        if (request.Items.Count == 0)
            return Result<int>.Success(0);

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
                    var createArgs = dto.ToArticleMetadataCreateArgs(request.SourceId, request.DefaultLanguageCode);
                    var meta = ArticleMetadata.Create(
                        sourceId: createArgs.SourceId,
                        title: createArgs.Title,
                        url: createArgs.Url,
                        externalId: createArgs.ExternalId,
                        publishedAt: createArgs.PublishedAt,
                        languageCode: createArgs.LanguageCode,
                        positivityScore: createArgs.PositivityScore,
                        author: createArgs.Author,
                        summaryShort: createArgs.SummaryShort,
                        imageTag: createArgs.ImageTag);

                    var content = ArticleContent.Create(dto.ContentRaw, dto.ContentClean);
                    meta.AttachContent(content);

                    if (dto.Topics != null)
                    {
                        foreach (var topicName in dto.Topics)
                        {
                            if (request.TopicLookup.ByName.TryGetValue(topicName, out var topic))
                                meta.AddTopic(topic.Id);
                        }
                    }

                    pairs.Add((meta, dto));
                    articleWriteRepository.Add(meta);
                }
                catch (DomainException ex)
                {
                    logger.LogWarning(
                        ex,
                        "Domain invariant violation building article entity for external id: {ExternalId}",
                        dto.ExternalId);

                    return Result<int>.Failure(
                        new Error(
                            ErrorCodes.Ingestion.DomainInvariantViolation,
                            $"Domain invariant violation for article '{dto.ExternalId ?? dto.Title}': {ex.Message}",
                            ErrorType.Conflict));
                }
            }

            if (pairs.Count == 0)
                continue;

            var savedInChunk = await ingestionArticleBatchSaver.SaveAddedArticlesAsync(
                pairs.Select(p => p.Meta).ToList(),
                cancellationToken);

            if (savedInChunk == pairs.Count)
            {
                foreach (var (_, dto) in pairs)
                    logger.LogInformation("Ingested new article: {Title}", dto.Title);
            }
            else if (savedInChunk > 0)
            {
                logger.LogInformation(
                    "Ingested {SavedCount} of {TotalCount} new articles in chunk (duplicates skipped).",
                    savedInChunk,
                    pairs.Count);
            }

            totalSaved += savedInChunk;
        }

        return Result<int>.Success(totalSaved);
    }
}
