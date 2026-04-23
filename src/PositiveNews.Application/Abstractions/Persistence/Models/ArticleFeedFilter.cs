namespace PositiveNews.Application.Abstractions.Persistence.Models;

public sealed record ArticleFeedFilter(int Page, int PageSize, string? Topic);
