using FluentAssertions;
using PositiveNews.Domain.Exceptions;
using PositiveNews.Domain.ValueObjects;

namespace PositiveNews.Domain.Tests.ValueObjects;

public class ArticleUrlTests
{
    [Fact]
    public void Create_Should_TrimAndAcceptAbsoluteUrl_When_InputHasLeadingWhitespace()
    {
        var url = ArticleUrl.Create(" https://example.com/articles/1 ");

        url.Value.Should().Be("https://example.com/articles/1");
    }

    [Theory]
    [InlineData("")]
    [InlineData("relative/path")]
    public void Create_Should_ThrowInvalidArticleStateException_When_EmptyOrRelative(string value)
    {
        var act = () => ArticleUrl.Create(value);

        act.Should().Throw<InvalidArticleStateException>();
    }

    [Fact]
    public void ImplicitConversion_Should_ReturnUrlValue_When_AssignedToString()
    {
        string s = ArticleUrl.Create("https://example.com");

        s.Should().Be("https://example.com");
    }
}
