using MediatR;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.DTOs.Admin;
using PositiveNews.Application.Queries.Admin;

namespace PositiveNews.Application.QueryHandlers.Admin;

/// <summary>
/// Loads source rows for the admin management UI.
/// </summary>
public sealed class GetAdminSourcesQueryHandler(ISourceReadRepository sourceReadRepository)
    : IRequestHandler<GetAdminSourcesQuery, IReadOnlyList<SourceAdminItemDto>>
{
    public Task<IReadOnlyList<SourceAdminItemDto>> Handle(
        GetAdminSourcesQuery request,
        CancellationToken cancellationToken)
    {
        return sourceReadRepository.GetAdminSourceListAsync(cancellationToken);
    }
}
