using MediatR;
using PositiveNews.Application.Common;
using PositiveNews.Application.DTOs.Articles;

namespace PositiveNews.Application.Queries.Articles;

public sealed record GetArticleDetailQuery(long Id) : IRequest<Result<ArticleDetailDto>>;
