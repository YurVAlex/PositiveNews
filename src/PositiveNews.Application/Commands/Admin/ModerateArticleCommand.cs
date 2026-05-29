using MediatR;
using PositiveNews.Application.Common;

namespace PositiveNews.Application.Commands.Admin;

/// <summary>
/// Moderates the active state of a single article.
/// </summary>
public sealed record ModerateArticleCommand(
    long ArticleId,
    bool IsActive,
    string? Reason,
    string? Note,
    long ModeratorId)
    : IRequest<Result>;
