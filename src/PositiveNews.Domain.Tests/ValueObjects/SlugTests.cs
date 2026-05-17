using FluentAssertions;
using PositiveNews.Domain.Exceptions;
using PositiveNews.Domain.ValueObjects;

namespace PositiveNews.Domain.Tests.ValueObjects;

public class SlugTests
{
    [Fact]
    public void Create_Should_TrimAndNormalizeToLowerCase_When_InputHasMixedCaseAndSpaces()
    {
        var slug = Slug.Create("  Good News  ");

        slug.Value.Should().Be("good news");
        slug.ToString().Should().Be("good news");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_ThrowDomainException_When_ValueIsNullOrWhitespace(string? value)
    {
        var act = () => Slug.Create(value);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ImplicitConversion_Should_ReturnSlugValue_When_AssignedToString()
    {
        string s = Slug.Create("hello");

        s.Should().Be("hello");
    }
}
