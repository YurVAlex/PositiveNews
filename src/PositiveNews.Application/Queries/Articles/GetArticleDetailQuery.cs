using MediatR;
using PositiveNews.Application.DTOs.Articles;

namespace PositiveNews.Application.Queries.Articles;

public sealed record GetArticleDetailQuery(long Id) : IRequest<ArticleDetailDto?>;
