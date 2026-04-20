using MediatR;
using Microsoft.EntityFrameworkCore;
using PositiveNews.Application.Abstractions.Persistence;
using PositiveNews.Application.DTOs.Articles;
using PositiveNews.Application.Queries.Articles;

namespace PositiveNews.Application.QueryHandlers.Articles;

public sealed class GetArticleDetailQueryHandler(IIngestionDbContext db)
    : IRequestHandler<GetArticleDetailQuery, ArticleDetailDto?>
{
    public async Task<ArticleDetailDto?> Handle(GetArticleDetailQuery request, CancellationToken cancellationToken)
    {
        var article = await db.ArticlesMetadata
            .Include(a => a.Source)
            .Include(a => a.Content)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.Id && a.IsActive, cancellationToken);

        if (article == null)
        {
            return null;
        }

        return new ArticleDetailDto
        {
            Id = article.Id,
            Title = article.Title,
            SourceName = article.Source.Name,
            SourceLogoUrl = article.Source.LogoUrl,
            Author = article.Author,
            PublishedAt = article.PublishedAt,
            ContentHtml = article.Content?.ContentRaw
        };
    }
}
