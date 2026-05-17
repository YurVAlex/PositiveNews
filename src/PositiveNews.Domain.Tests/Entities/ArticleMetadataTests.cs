using FluentAssertions;
using PositiveNews.Domain.Entities;
using PositiveNews.Domain.Exceptions;
using PositiveNews.Domain.ValueObjects;

namespace PositiveNews.Domain.Tests.Entities;

public class ArticleMetadataTests
{
    [Fact]
    public void Create_Should_SetFieldsAndDefaults_When_ValidInputProvided()
    {
        var article = ArticleMetadata.Create(
            sourceId: 1,
            title: " Hello World ",
            url: " https://example.com/1 ",
            externalId: " ext123 ",
            publishedAt: new DateTime(2023, 1, 1),
            languageCode: " en-US ",
            positivityScore: 0.75m,
            author: " YurVALex ",
            summaryShort: " This is a test summary ",
            imageTag: " <img>https://fakeUrl.com/fake.jpg<img/> ");

        article.SourceId.Should().Be(1);
        article.Title.Should().Be("Hello World");
        article.Url.Should().Be("https://example.com/1");
        article.ExternalId.Should().Be("ext123");
        article.PublishedAt.Should().Be(new DateTime(2023, 1, 1));
        article.LanguageCode.Should().Be("en-US");
        article.RegionCode.Should().Be("Global");
        article.PositivityScore.Should().Be(0.75m);
        article.AnalyzedAt.Should().NotBeNull();
        article.AnalyzedAt!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        article.Author.Should().Be("YurVALex");
        article.SummaryShort.Should().Be(" This is a test summary ");
        article.ImageTag.Should().Be(" <img>https://fakeUrl.com/fake.jpg<img/> ");
        article.IngestedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        article.IsActive.Should().BeTrue();
        article.ViewCount.Should().Be(0);
    }

    [Fact]
    public void Create_Should_TruncateTitle_When_TitleExceeds500Characters()
    {
        var longTitle = new string('x', 600);
        var article = ArticleMetadata.Create(1, longTitle, "http://x", null, DateTime.UtcNow, "en");

        article.Title.Length.Should().Be(500);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_ThrowInvalidArticleStateException_When_TitleEmpty(string? title)
    {
        var act = () => ArticleMetadata.Create(1, title!, "http://x", null, DateTime.UtcNow, "en");

        act.Should().Throw<InvalidArticleStateException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_ThrowInvalidArticleStateException_When_UrlEmpty(string? url)
    {
        var act = () => ArticleMetadata.Create(1, "title", url!, null, DateTime.UtcNow, "en");

        act.Should().Throw<InvalidArticleStateException>();
    }

    [Fact]
    public void Create_Should_ThrowInvalidArticleStateException_When_PositivityOutOfRange()
    {
        var act = () => ArticleMetadata.Create(1, "t", "http://x", null, DateTime.UtcNow, "en", 1.5m);

        act.Should().Throw<InvalidArticleStateException>();
    }

    [Fact]
    public void Create_Should_LeaveScoreAndAnalyzedAtNull_When_PositivityNotProvided()
    {
        var article = ArticleMetadata.Create(1, "t", "http://x", null, DateTime.UtcNow, "en", null);

        article.PositivityScore.Should().BeNull();
        article.AnalyzedAt.Should().BeNull();
    }

    [Fact]
    public void AttachContent_Should_LinkOnceAndRejectSecondAttach_When_ContentAlreadySet()
    {
        var article = ArticleMetadata.Create(1, "t", "http://x", null, DateTime.UtcNow, "en");
        var content = ArticleContent.Create("raw", "clean");

        article.AttachContent(content);

        article.Content.Should().Be(content);

        var act = () => article.AttachContent(ArticleContent.Create("r2", "c2"));

        act.Should().Throw<InvalidArticleStateException>();
    }

    [Fact]
    public void AttachContent_Should_ThrowArgumentNullException_When_ContentNull()
    {
        var article = ArticleMetadata.Create(1, "t", "http://x", null, DateTime.UtcNow, "en");

        var act = () => article.AttachContent(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Deactivate_Should_MarkInactiveAndPreventSecondCall_When_AlreadyInactive()
    {
        var article = ArticleMetadata.Create(1, "t", "http://x", null, DateTime.UtcNow, "en");

        article.Deactivate(10);

        article.IsActive.Should().BeFalse();
        article.ModeratedBy.Should().Be(10);

        var act = () => article.Deactivate(10);

        act.Should().Throw<InvalidArticleStateException>();
    }

    [Fact]
    public void AddTopic_Should_AddDistinctTopicIdsOnly_When_DuplicateRequested()
    {
        var article = ArticleMetadata.Create(1, "t", "http://x", null, DateTime.UtcNow, "en");

        article.AddTopic(1);
        article.AddTopic(2);
        article.ArticleTopics.Should().HaveCount(2);

        article.AddTopic(1);

        article.ArticleTopics.Should().HaveCount(2);
    }

    [Fact]
    public void IncrementViewCount_Should_IncreaseByOne_When_CalledRepeatedly()
    {
        var article = ArticleMetadata.Create(1, "t", "http://x", null, DateTime.UtcNow, "en");

        article.IncrementViewCount();
        article.IncrementViewCount();

        article.ViewCount.Should().Be(2);
    }

    [Fact]
    public void PositivityScore_Invalid_Value_Should_ThrowInvalidArticleStateException()
    {
        var act = () => ArticleMetadata.Create(999999, "Test title", "http://My-test-URL", 
                        "GUID-SOME-GUID-OR-URL-OR-SOME string", DateTime.UtcNow, "en", -0.1m);

        act.Should().Throw<InvalidArticleStateException>();
    }
}
