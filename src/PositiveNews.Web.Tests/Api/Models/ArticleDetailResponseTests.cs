using FluentAssertions;
using PositiveNews.Web.Api.Models;

namespace PositiveNews.Web.Tests.Api.Models;

public class ArticleDetailResponseTests
{
    [Fact]
    public void ArticleDetailResponse_Should_AllowNullOptionalFields_When_Constructed()
    {
        var sut = new ArticleDetailResponse
        {
            Id = 1,
            Title = "T",
            SourceName = "S",
            SourceLogoUrl = null,
            Author = null,
            PublishedAt = DateTime.UtcNow,
            ContentHtml = null
        };

        sut.SourceLogoUrl.Should().BeNull();
        sut.Author.Should().BeNull();
        sut.ContentHtml.Should().BeNull();
    }
}
