using FluentAssertions;
using PositiveNews.Web.Api.Models;

namespace PositiveNews.Web.Tests.Api.Models;

public class ArticleFeedResponseTests
{
    [Fact]
    public void ArticleFeedResponse_Should_HoldPagingMetadata_When_Constructed()
    {
        var sut = new ArticleFeedResponse
        {
            Articles = [],
            CurrentPage = 2,
            TotalPages = 10,
            PageSize = 20,
            SelectedTopics = ["Tech"]
        };

        sut.CurrentPage.Should().Be(2);
        sut.TotalPages.Should().Be(10);
        sut.PageSize.Should().Be(20);
        sut.SelectedTopics.Should().ContainSingle("Tech");
        sut.Articles.Should().BeEmpty();
    }
}
