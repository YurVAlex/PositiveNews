using MediatR;
using PositiveNews.Application.Common;

namespace PositiveNews.Application.Commands.Admin;

/// <summary>
/// Updates a source record from the admin edit UI.
/// </summary>
public sealed record UpdateSourceCommand(
    int SourceId,
    decimal TrustScore,
    bool IsActive,
    string FeedUrl,
    string? Reason,
    string? Note,
    long ModeratorId)
    : IRequest<Result>;
