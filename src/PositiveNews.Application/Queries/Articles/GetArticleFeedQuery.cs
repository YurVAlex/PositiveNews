using MediatR;
using PositiveNews.Application.Abstractions.Persistence.Models;
using PositiveNews.Application.Common;
using PositiveNews.Application.DTOs.Articles;

namespace PositiveNews.Application.Queries.Articles;

/// <summary>
/// Requests one page of articles for the public feed with optional topic filters and sort order.
/// </summary>
/// <param name="Page">One-based page index.</param>
/// <param name="Topics">Optional topic names filter (null means no filter).</param>
/// <param name="SourceIds">Optional preferred source ids (null means no preference).</param>
/// <param name="PageSize">Items per page.</param>
/// <param name="SortBy">Primary sort column.</param>
public sealed record GetArticleFeedQuery(
        int Page = 1,
        IReadOnlyList<string>? Topics = null,
        IReadOnlyList<int>? SourceIds = null,
        int PageSize = 10,
        ArticleFeedSortBy SortBy = ArticleFeedSortBy.PublishedAt)
    : IRequest<Result<ArticleFeedPageResult>>;
