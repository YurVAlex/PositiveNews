using MediatR;
using PositiveNews.Application.Abstractions.Persistence.Models;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.Common;
using PositiveNews.Application.DTOs.Articles;
using PositiveNews.Application.Queries.Articles;

namespace PositiveNews.Application.QueryHandlers.Articles;

/// <summary>
/// Projects repository feed queries using paging, topic filters, and sort options from the query.
/// </summary>
/// <param name="articleReadRepository">Paged article feed access.</param>
/// <param name="topicReadRepository">Topic taxonomy read access used for filter validation.</param>
public sealed class GetArticleFeedQueryHandler(
    IArticleReadRepository articleReadRepository,
    ITopicReadRepository topicReadRepository)
    : IRequestHandler<GetArticleFeedQuery, Result<ArticleFeedPageResult>>
{
    /// <summary>
    /// Maps the MediatR query into an <see cref="PositiveNews.Application.Abstractions.Persistence.Models.ArticleFeedFilter"/> and loads one page.
    /// </summary>
    /// <param name="request">Paging and filter parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paged feed result for the UI or a typed not-found error when page/topics are unknown.</returns>
    public async Task<Result<ArticleFeedPageResult>> Handle(GetArticleFeedQuery request, CancellationToken cancellationToken)
    {
        var topics = NormalizeTopics(request.Topics);
        if (topics.Count > 0)
        {
            var knownTopics = await topicReadRepository.GetTopicNamesAsync(cancellationToken);
            var knownTopicSet = new HashSet<string>(knownTopics, StringComparer.OrdinalIgnoreCase);
            var missingTopics = topics.Where(topic => !knownTopicSet.Contains(topic)).ToArray();
            if (missingTopics.Length > 0)
            {
                return Result<ArticleFeedPageResult>.Failure(
                    new Error(
                        "ArticleFeed.TopicNotFound",
                        $"Requested topic(s) were not found: {string.Join(", ", missingTopics)}.",
                        ErrorType.NotFound));
            }
        }
        var filter = new ArticleFeedFilter(
            request.Page,
            request.PageSize,
            topics,
            request.SortBy);
        var page = await articleReadRepository.GetFeedPageAsync(filter, cancellationToken);
        var maxPage = Math.Max(1, page.TotalPages);

        if (request.Page > maxPage)
        {
            return Result<ArticleFeedPageResult>.Failure(
                new Error(
                    "ArticleFeed.PageNotFound",
                    $"Requested page '{request.Page}' does not exist for the selected filter.",
                    ErrorType.NotFound));
        }

        return Result<ArticleFeedPageResult>.Success(page);
    }

    private static IReadOnlyList<string> NormalizeTopics(IReadOnlyList<string>? topics)
    {
        return (topics ?? Array.Empty<string>())
            .Where(topic => !string.IsNullOrWhiteSpace(topic))
            .Select(topic => topic.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
