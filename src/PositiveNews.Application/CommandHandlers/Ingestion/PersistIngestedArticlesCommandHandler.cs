using MediatR;
using Microsoft.Extensions.Logging;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Write;
using PositiveNews.Application.Abstractions.Persistence.UnitOfWork;
using PositiveNews.Application.Commands.Ingestion;
using PositiveNews.Application.DTOs;
using PositiveNews.Application.Ingestion;
using PositiveNews.Application.Mapping;
using PositiveNews.Domain.Entities;
using PositiveNews.Domain.Exceptions;

namespace PositiveNews.Application.CommandHandlers.Ingestion;

public sealed class PersistIngestedArticlesCommandHandler(
    IArticleWriteRepository articleWriteRepository,
    ITopicReadRepository topicReadRepository,
    IIngestionUnitOfWork ingestionUnitOfWork,
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

            var distinctTopicNames = chunk
                .SelectMany(dto => dto.Topics ?? Enumerable.Empty<string>())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var topicIdsByName = await topicReadRepository.GetTopicIdsByNamesAsync(distinctTopicNames, cancellationToken);

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
                            if (topicIdsByName.TryGetValue(topicName, out var topicId))
                                meta.AddTopic(topicId);
                        }
                    }

                    pairs.Add((meta, dto));
                    articleWriteRepository.Add(meta);
                }
                catch (DomainException ex)
                {
                    logger.LogWarning(ex, "Domain invariant violation building article entity for external id: {ExternalId}", dto.ExternalId);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogError(ex, "Error building article entity for external id: {ExternalId}", dto.ExternalId);
                }
            }

            if (pairs.Count == 0)
                continue;

            await ingestionUnitOfWork.SaveChangesAsync(cancellationToken);

            foreach (var (_, dto) in pairs)
                logger.LogInformation("Ingested new article: {Title}", dto.Title);

            totalSaved += pairs.Count;
        }

        return totalSaved;
    }
}
