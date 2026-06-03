using MediatR;
using PositiveNews.Application.Common;
using PositiveNews.Application.DTOs.Admin;

namespace PositiveNews.Application.Queries.Admin;

public sealed record GetAuditLogsQuery(int Limit = 100) : IRequest<Result<IReadOnlyList<AuditLogAdminItemDto>>>;
