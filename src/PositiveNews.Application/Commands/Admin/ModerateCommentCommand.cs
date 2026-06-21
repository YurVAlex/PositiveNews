using MediatR;
using PositiveNews.Application.Common;

namespace PositiveNews.Application.Commands.Admin;

public sealed record ModerateCommentCommand(
    long CommentId,
    bool IsActive,
    string? Reason,
    string? Note,
    long ModeratorId) : IRequest<Result>;
