using FluentAssertions;
using PositiveNews.Domain.Entities;

namespace PositiveNews.Domain.Tests.Entities;

public class ArticleTopicTests
{
    [Fact]
    public void Create_Should_SetArticleAndTopicIds_When_ByIds()
    {
        var at = ArticleTopic.Create(101, 202);

        at.ArticleId.Should().Be(101);
        at.TopicId.Should().Be(202);
    }

    [Fact]
    public void Create_Should_SetNavigationAndTopicId_When_ArticleProvided()
    {
        var article = ArticleMetadata.Create(1, "t", "http://x", null, DateTime.UtcNow, "en");

        var at = ArticleTopic.Create(article, 55);

        at.Article.Should().Be(article);
        at.TopicId.Should().Be(55);
    }

    [Fact]
    public void Create_Should_ThrowArgumentNullException_When_ArticleNull()
    {
        var act = () => ArticleTopic.Create((ArticleMetadata)null!, 1);

        act.Should().Throw<ArgumentNullException>();
    }
}
