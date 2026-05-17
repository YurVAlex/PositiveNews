using FluentAssertions;
using PositiveNews.Domain.Entities;

namespace PositiveNews.Domain.Tests.Entities;

public class ArticleContentTests
{
    [Fact]
    public void Create_Should_StoreRawAndClean_When_ValuesProvided()
    {
        var content = ArticleContent.Create("<p>raw</p>", "<p>clean</p>");

        content.ContentRaw.Should().Be("<p>raw</p>");
        content.ContentClean.Should().Be("<p>clean</p>");
    }

    [Fact]
    public void UpdateContent_Should_OverwriteRawAndClean_When_Called()
    {
        var content = ArticleContent.Create("old", "old");

        content.UpdateContent("new raw", "new clean");

        content.ContentRaw.Should().Be("new raw");
        content.ContentClean.Should().Be("new clean");
    }
}
