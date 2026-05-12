using MediatR;
using PositiveNews.Application.Common;
using PositiveNews.Application.DTOs.Articles;

namespace PositiveNews.Application.Queries.Articles;

/// <summary>
/// Loads a single article by identifier for the detail view.
/// </summary>
/// <param name="Id">Article primary key.</param>
public sealed record GetArticleDetailQuery(long Id) : IRequest<Result<ArticleDetailDto>>;
