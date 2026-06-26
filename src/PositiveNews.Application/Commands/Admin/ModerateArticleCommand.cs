using MediatR;
using PositiveNews.Application.Common;

namespace PositiveNews.Application.Commands.Admin;

/// <summary>
/// Moderates a single article and applies optional metadata or content updates.
/// </summary>
public sealed record ModerateArticleCommand(
    long ArticleId,
    bool IsActive,
    string? Title,
    string? ImageTag,
    decimal? PositivityScore,
    string? SummaryShort,
    string? ContentRaw,
    string? Reason,
    string? Note,
    long ModeratorId)
    : IRequest<Result>;
