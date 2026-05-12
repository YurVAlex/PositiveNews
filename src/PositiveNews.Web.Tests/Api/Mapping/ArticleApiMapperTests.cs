using FluentAssertions;
using PositiveNews.Application.DTOs.Articles;
using PositiveNews.Web.Api.Mapping;
using PositiveNews.Web.Tests.TestHelpers;

namespace PositiveNews.Web.Tests.Api.Mapping;

public class ArticleApiMapperTests
{
    [Fact]
    public void ToArticlePreviewResponse_Should_MapAllFields_When_DtoProvided()
    {
        var dto = TestDataBuilders.ArticlePreviewDto(id: 9, title: "T");

        var preview = dto.ToArticlePreviewResponse();

        preview.Id.Should().Be(9);
        preview.Title.Should().Be("T");
        preview.SourceName.Should().Be(dto.SourceName);
        preview.SourceLogoUrl.Should().Be(dto.SourceLogoUrl);
        preview.Author.Should().Be(dto.Author);
        preview.PublishedAt.Should().Be(dto.PublishedAt);
        preview.ImageTag.Should().Be(dto.ImageTag);
        preview.SummaryShort.Should().Be(dto.SummaryShort);
        preview.Url.Should().Be(dto.Url);
        preview.PositivityScore.Should().Be(dto.PositivityScore);
        preview.Topics.Should().BeEquivalentTo(dto.Topics);
    }

    [Fact]
    public void ToArticleDetailResponse_Should_MapAllFields_When_DtoProvided()
    {
        var dto = TestDataBuilders.ArticleDetail();

        var detail = dto.ToArticleDetailResponse();

        detail.Id.Should().Be(dto.Id);
        detail.Title.Should().Be(dto.Title);
        detail.SourceName.Should().Be(dto.SourceName);
        detail.SourceLogoUrl.Should().Be(dto.SourceLogoUrl);
        detail.Author.Should().Be(dto.Author);
        detail.PublishedAt.Should().Be(dto.PublishedAt);
        detail.ContentHtml.Should().Be(dto.ContentHtml);
    }

    [Fact]
    public void ToArticleFeedResponse_Should_MapPagingAndArticles_When_PageResultProvided()
    {
        var source = TestDataBuilders.ArticleFeedPage(currentPage: 2, totalPages: 5, pageSize: 10);

        var feed = source.ToArticleFeedResponse();

        feed.CurrentPage.Should().Be(2);
        feed.TotalPages.Should().Be(5);
        feed.PageSize.Should().Be(10);
        feed.SelectedTopics.Should().BeEquivalentTo(source.SelectedTopics);
        feed.Articles.Should().HaveCount(source.Articles.Count);
    }
}
