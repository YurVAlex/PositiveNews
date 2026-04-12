using MediatR;
using PositiveNews.Application.DTOs;

namespace PositiveNews.Application.Commands.Ingestion;

/// <summary>
/// Persists new articles (and topic links) for one source. Returns count successfully saved.
/// </summary>
public sealed record PersistIngestedArticlesCommand(
    int SourceId,
    string DefaultLanguageCode,
    IReadOnlyList<RssFeedItemDto> Items) : IRequest<int>;
