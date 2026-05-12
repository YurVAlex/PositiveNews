using MediatR;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.Queries.Articles;

namespace PositiveNews.Application.QueryHandlers.Articles;

/// <summary>
/// Supplies ordered topic names for feed filter controls.
/// </summary>
/// <param name="topicReadRepository">Reads topic taxonomy names.</param>
public sealed class GetTopicFilterListQueryHandler(ITopicReadRepository topicReadRepository)
    : IRequestHandler<GetTopicFilterListQuery, IReadOnlyList<string>>
{
    /// <summary>
    /// Delegates to the repository topic name listing used by the filter UI.
    /// </summary>
    /// <param name="request">Marker query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Topic names suitable for display.</returns>
    public Task<IReadOnlyList<string>> Handle(GetTopicFilterListQuery request, CancellationToken cancellationToken)
    {
        return topicReadRepository.GetTopicNamesAsync(cancellationToken);
    }
}
