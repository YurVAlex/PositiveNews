using MediatR;
using Microsoft.EntityFrameworkCore;
using PositiveNews.Application.Abstractions.Persistence;
using PositiveNews.Application.DTOs.Articles;
using PositiveNews.Application.Queries.Articles;

namespace PositiveNews.Application.QueryHandlers.Articles;

public sealed class GetArticleFeedQueryHandler(IIngestionDbContext db)
    : IRequestHandler<GetArticleFeedQuery, ArticleFeedPageResult>
{
    public async Task<ArticleFeedPageResult> Handle(GetArticleFeedQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var topic = string.IsNullOrWhiteSpace(request.Topic) ? null : request.Topic.Trim();

        var query = db.ArticlesMetadata
            .Include(a => a.Source)
            .Include(a => a.ArticleTopics)
                .ThenInclude(at => at.Topic)
            .Where(a => a.IsActive)
            .AsNoTracking();

        if (topic != null)
        {
            query = query
                .OrderByDescending(a => a.ArticleTopics.Any(at => at.Topic!.Name == topic))
                .ThenByDescending(a => a.PublishedAt);
        }
        else
        {
            query = query.OrderByDescending(a => a.PublishedAt);
        }

        var totalArticles = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalArticles / (double)pageSize);

        var articles = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new ArticleFeedItemDto
            {
                Id = a.Id,
                SourceName = a.Source.Name,
                SourceLogoUrl = a.Source.LogoUrl,
                Title = a.Title,
                Author = a.Author,
                PublishedAt = a.PublishedAt,
                ImageTag = a.ImageTag,
                SummaryShort = a.SummaryShort ?? "No summary available.",
                Topics = a.ArticleTopics
                    .Where(at => at.Topic != null)
                    .Select(at => at.Topic!.Name)
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        return new ArticleFeedPageResult
        {
            Articles = articles,
            CurrentPage = page,
            TotalPages = totalPages,
            PageSize = pageSize,
            SelectedTopic = topic
        };
    }
}
