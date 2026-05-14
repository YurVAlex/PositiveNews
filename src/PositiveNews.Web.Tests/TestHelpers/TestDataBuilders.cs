using PositiveNews.Application.Abstractions.Persistence.Models;
using PositiveNews.Application.DTOs.Articles;
using PositiveNews.Application.Features.Auth.Models;

namespace PositiveNews.Web.Tests.TestHelpers;

internal static class TestDataBuilders
{
    public static AuthResultModel AuthResult(
        string token = "test-access-token",
        DateTime? expiresAtUtc = null,
        UserProfileModel? user = null)
        => new()
        {
            AccessToken = token,
            ExpiresAtUtc = expiresAtUtc ?? DateTime.UtcNow.AddHours(1),
            User = user ?? UserProfile()
        };

    public static UserProfileModel UserProfile(
        long id = 1,
        string email = "user@test.com",
        string name = "Test User",
        IReadOnlyList<string>? roles = null)
        => new()
        {
            Id = id,
            Email = email,
            Name = name,
            Roles = roles ?? ["User"]
        };

    public static ArticleFeedPageResult ArticleFeedPage(
        int currentPage = 1,
        int totalPages = 3,
        int pageSize = 10,
        IReadOnlyList<string>? selectedTopics = null,
        IReadOnlyList<ArticleFeedItemDto>? articles = null)
        => new()
        {
            CurrentPage = currentPage,
            TotalPages = totalPages,
            PageSize = pageSize,
            SelectedTopics = selectedTopics ?? [],
            Articles = articles ?? [ArticlePreviewDto()]
        };

    public static ArticleFeedItemDto ArticlePreviewDto(long id = 1, string title = "Headline")
        => new()
        {
            Id = id,
            SourceName = "Source",
            SourceLogoUrl = "https://logo",
            SourceTrustScore = 0.95m,
            Title = title,
            Author = "Author",
            PublishedAt = DateTime.UtcNow,
            ImageTag = null,
            SummaryShort = "Summary",
            Url = "https://example.com/a",
            PositivityScore = 0.5m,
            Topics = ["Tech"]
        };

    public static ArticleDetailDto ArticleDetail(long id = 42)
        => new()
        {
            Id = id,
            Title = "Detail title",
            SourceName = "Src",
            SourceLogoUrl = null,
            Author = "A",
            PublishedAt = DateTime.UtcNow,
            ContentHtml = "<p>html</p>"
        };
}
