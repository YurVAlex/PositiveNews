using MediatR;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.DTOs.Articles;
using PositiveNews.Application.Queries.Articles;

namespace PositiveNews.Application.QueryHandlers.Articles;

/// <summary>
/// Supplies ordered source rows for feed filter controls.
/// </summary>
/// <param name="sourceReadRepository">Reads source catalog rows.</param>
public sealed class GetSourceFilterListQueryHandler(ISourceReadRepository sourceReadRepository)
    : IRequestHandler<GetSourceFilterListQuery, IReadOnlyList<SourceFilterItemDto>>
{
    /// <inheritdoc />
    public Task<IReadOnlyList<SourceFilterItemDto>> Handle(
        GetSourceFilterListQuery request,
        CancellationToken cancellationToken)
    {
        return sourceReadRepository.GetSourceFilterListAsync(cancellationToken);
    }
}
