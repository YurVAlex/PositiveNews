using MediatR;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.Common;
using PositiveNews.Application.DTOs.Admin;
using PositiveNews.Application.Queries.Admin;

namespace PositiveNews.Application.QueryHandlers.Admin;

public sealed class GetAuditLogsQueryHandler(IAuditLogReadRepository auditLogReadRepository)
    : IRequestHandler<GetAuditLogsQuery, Result<IReadOnlyList<AuditLogAdminItemDto>>>
{
    public async Task<Result<IReadOnlyList<AuditLogAdminItemDto>>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var items = await auditLogReadRepository.GetRecentAuditLogsAsync(request.Limit, cancellationToken);
        return Result<IReadOnlyList<AuditLogAdminItemDto>>.Success(items);
    }
}
