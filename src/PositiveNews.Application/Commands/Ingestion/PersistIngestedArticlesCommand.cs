using MediatR;
using PositiveNews.Application.Common;
using PositiveNews.Application.DTOs.Ingestion;

namespace PositiveNews.Application.Commands.Ingestion;

/// <summary>
/// Persists new articles (and topic links) for one source. Returns count successfully saved.
/// </summary>
/// <param name="SourceId">News source identifier articles belong to.</param>
/// <param name="DefaultLanguageCode">Language code applied when creating metadata.</param>
/// <param name="TopicLookup">Pre-built lookup for resolving topic names to identifiers.</param>
/// <param name="Items">Parsed feed items that passed deduplication.</param>
public sealed record PersistIngestedArticlesCommand(
    int SourceId,
    string DefaultLanguageCode,
    TopicLookup TopicLookup,
    IReadOnlyList<RssFeedItemDto> Items) : IRequest<Result<int>>;
