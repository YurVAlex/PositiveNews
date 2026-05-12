using FluentAssertions;
using PositiveNews.Domain.Exceptions;
using PositiveNews.Domain.ValueObjects;

namespace PositiveNews.Domain.Tests.ValueObjects;

public class LanguageCodeTests
{
    [Fact]
    public void Create_Should_TrimValue_When_InputHasWhitespace()
    {
        var code = LanguageCode.Create("  en-US  ");

        code.Value.Should().Be("en-US");
    }

    [Fact]
    public void Und_Should_ReturnUndeterminedPlaceholder_When_Accessed()
    {
        LanguageCode.Und.Value.Should().Be("und");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_ThrowDomainException_When_ValueIsNullOrWhitespace(string? value)
    {
        var act = () => LanguageCode.Create(value);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_Should_ThrowDomainException_When_ValueExceedsMaxLength()
    {
        var act = () => LanguageCode.Create("abcdefghijk");

        act.Should().Throw<DomainException>().WithMessage("*too long*");
    }
}
