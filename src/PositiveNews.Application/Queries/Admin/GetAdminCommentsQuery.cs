using MediatR;
using PositiveNews.Application.Common;
using PositiveNews.Application.DTOs.Admin;

namespace PositiveNews.Application.Queries.Admin;

public sealed record GetAdminCommentsQuery : IRequest<Result<IReadOnlyList<CommentAdminItemDto>>>;
