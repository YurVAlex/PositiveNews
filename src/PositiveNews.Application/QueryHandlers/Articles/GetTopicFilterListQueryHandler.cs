using MediatR;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.Queries.Articles;

namespace PositiveNews.Application.QueryHandlers.Articles;

public sealed class GetTopicFilterListQueryHandler(ITopicReadRepository topicReadRepository)
    : IRequestHandler<GetTopicFilterListQuery, IReadOnlyList<string>>
{
    public Task<IReadOnlyList<string>> Handle(GetTopicFilterListQuery request, CancellationToken cancellationToken)
    {
        return topicReadRepository.GetTopicNamesAsync(cancellationToken);
    }
}
