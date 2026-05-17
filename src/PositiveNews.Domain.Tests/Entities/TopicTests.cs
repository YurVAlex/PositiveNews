using FluentAssertions;
using PositiveNews.Domain.Entities;
using PositiveNews.Domain.Exceptions;

namespace PositiveNews.Domain.Tests.Entities;

public class TopicTests
{
    [Fact]
    public void Create_Should_TrimNameAndLowercaseSlug_When_InputHasMixedCase()
    {
        var topic = Topic.Create("  Health  ", "  HEALTH-SLUG  ", " desc ");

        topic.Name.Should().Be("Health");
        topic.Slug.Should().Be("health-slug");
        topic.Description.Should().Be(" desc ");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_ThrowDomainException_When_NameEmpty(string? name)
    {
        var act = () => Topic.Create(name!, "slug");

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_ThrowDomainException_When_SlugEmpty(string? slug)
    {
        var act = () => Topic.Create("name", slug!);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Update_Should_ChangeNameAndDescription_When_ValidInput()
    {
        var topic = Topic.Create("Old", "old-slug");

        topic.Update("  New  ", "new desc");

        topic.Name.Should().Be("New");
        topic.Description.Should().Be("new desc");
    }

    [Fact]
    public void Update_Should_ThrowDomainException_When_NameEmpty()
    {
        var topic = Topic.Create("Old", "old-slug");

        var act = () => topic.Update("", null);

        act.Should().Throw<DomainException>();
    }
}
