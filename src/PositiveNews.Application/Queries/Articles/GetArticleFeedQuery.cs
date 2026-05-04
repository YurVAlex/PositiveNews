using MediatR;
using PositiveNews.Application.Abstractions.Persistence.Models;
using PositiveNews.Application.DTOs.Articles;

namespace PositiveNews.Application.Queries.Articles;

public sealed record GetArticleFeedQuery(
        int Page = 1,
        IReadOnlyList<string>? Topics = null,
        int PageSize = 10,
        ArticleFeedSortBy SortBy = ArticleFeedSortBy.PublishedAt)
    : IRequest<ArticleFeedPageResult>;
