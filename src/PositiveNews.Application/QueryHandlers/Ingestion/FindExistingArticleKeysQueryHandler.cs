using MediatR;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.DTOs;
using PositiveNews.Application.Queries.Ingestion;

namespace PositiveNews.Application.QueryHandlers.Ingestion;

public sealed class FindExistingArticleKeysQueryHandler(IArticleReadRepository articleReadRepository)
    : IRequestHandler<FindExistingArticleKeysQuery, ExistingArticleKeys>
{
    public Task<ExistingArticleKeys> Handle(
        FindExistingArticleKeysQuery request,
        CancellationToken cancellationToken)
    {
        return articleReadRepository.FindExistingKeysAsync(
            request.ExternalIds,
            request.Urls,
            request.Titles,
            cancellationToken);
    }
}
