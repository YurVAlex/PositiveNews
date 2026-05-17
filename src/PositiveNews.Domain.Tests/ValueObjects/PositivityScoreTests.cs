using FluentAssertions;
using PositiveNews.Domain.Exceptions;
using PositiveNews.Domain.ValueObjects;
using System.Globalization;

namespace PositiveNews.Domain.Tests.ValueObjects;

public class PositivityScoreTests
{
    [Fact]
    public void Create_Should_RoundToFourDecimals_When_ValueHasExtraPrecision()
    {
        var score = PositivityScore.Create(0.12345m);

        score.Value.Should().Be(0.1234m);
        score.ToString().Should().Be(0.1234m.ToString("F4", CultureInfo.CurrentCulture));
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    public void Create_Should_ThrowInvalidArticleStateException_When_ValueOutsideZeroToOne(double value)
    {
        var act = () => PositivityScore.Create((decimal)value);

        act.Should().Throw<InvalidArticleStateException>()
            .WithMessage("*PositivityScore*");
    }

    [Fact]
    public void ImplicitConversion_Should_ReturnUnderlyingDecimal_When_AssignedToDecimal()
    {
        decimal d = PositivityScore.Create(0.5m);

        d.Should().Be(0.5m);
    }
}
