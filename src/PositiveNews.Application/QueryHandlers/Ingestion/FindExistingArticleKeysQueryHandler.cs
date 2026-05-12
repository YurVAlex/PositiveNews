using MediatR;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.DTOs;
using PositiveNews.Application.Queries.Ingestion;

namespace PositiveNews.Application.QueryHandlers.Ingestion;

/// <summary>
/// Loads deduplication key sets from persistence for a batch of candidate articles.
/// </summary>
/// <param name="articleReadRepository">Article read queries including bulk key lookup.</param>
public sealed class FindExistingArticleKeysQueryHandler(IArticleReadRepository articleReadRepository)
    : IRequestHandler<FindExistingArticleKeysQuery, ExistingArticleKeys>
{
    /// <summary>
    /// Forwards the repository call with external IDs, URLs, and titles from the query.
    /// </summary>
    /// <param name="request">Batch key inputs.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Existing key sets for duplicate detection.</returns>
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
