using FluentAssertions;
using PositiveNews.Domain.Exceptions;
using PositiveNews.Domain.ValueObjects;

namespace PositiveNews.Domain.Tests.ValueObjects;

public class NonEmptyStringTests
{
    [Fact]
    public void Create_Should_TrimAndWrap_When_InputHasPadding()
    {
        var s = NonEmptyString.Create("  hello  ");

        s.Value.Should().Be("hello");
    }

    [Fact]
    public void Create_Should_TrimMultiWordContent_When_InputHasPadding()
    {
        var nes = NonEmptyString.Create("  hello world  ");

        nes.Value.Should().Be("hello world");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_ThrowDomainException_When_ValueIsNullOrWhitespace(string? value)
    {
        var act = () => NonEmptyString.Create(value);

        act.Should().Throw<DomainException>().WithMessage("*value*");
    }

    [Fact]
    public void Create_Should_UseFieldNameInMessage_When_CustomFieldNameProvided()
    {
        var act = () => NonEmptyString.Create(" ", fieldName: "Title");

        act.Should().Throw<DomainException>().WithMessage("*Title*");
    }

    [Fact]
    public void ImplicitConversion_Should_ReturnValue_When_AssignedToString()
    {
        string s = NonEmptyString.Create("test");

        s.Should().Be("test");
    }
}
