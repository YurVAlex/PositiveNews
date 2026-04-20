using MediatR;
using PositiveNews.Application.DTOs.Articles;

namespace PositiveNews.Application.Queries.Articles;

public sealed record GetArticleFeedQuery(int Page = 1, string? Topic = null, int PageSize = 10)
    : IRequest<ArticleFeedPageResult>;
