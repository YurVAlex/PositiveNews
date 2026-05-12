using FluentAssertions;
using PositiveNews.Application.DTOs;
using PositiveNews.Infrastructure.Services;

namespace PositiveNews.Infrastructure.Tests.Services;

public class KeyPhrasePositivityAnalyzerTests
{
    private readonly KeyPhrasePositivityAnalyzer _sut = new();

    private static PositivityAnalizerKeyPhrases KeyPhrases() => new(
        new HashSet<string>(["good", "recovery"]),
        new HashSet<string>(["bad", "crisis", "harm"]),
        new HashSet<string>(["breakthrough"]),
        new HashSet<string>(["bad news"]),
        new HashSet<string>(["not"]),
        new HashSet<string>(["very"]),
        NegationLookbackTokens: 2,
        IntensifierLookbackTokens: 1,
        IntensifierMultiplier: 1.5m,
        PhrasePolarityWeight: 2m);

    [Fact]
    public void AnalyzeSentiment_Should_ReturnNeutral_When_TextEmpty()
    {
        _sut.AnalyzeSentiment("", KeyPhrases()).Should().Be(0.5000m);
        _sut.AnalyzeSentiment("   ", KeyPhrases()).Should().Be(0.5000m);
    }

    [Fact]
    public void AnalyzeSentiment_Should_RankPositiveAboveNegative_When_ClearSignals()
    {
        var positive = _sut.AnalyzeSentiment("A very good recovery is a breakthrough.", KeyPhrases());
        var negative = _sut.AnalyzeSentiment("A very bad crisis caused harm.", KeyPhrases());

        positive.Should().BeGreaterThan(0.5m);
        negative.Should().BeLessThan(0.5m);
        positive.Should().BeGreaterThan(negative);
    }

    [Fact]
    public void AnalyzeSentiment_Should_HandleNegation_When_NotBadPhrase()
    {
        var score = _sut.AnalyzeSentiment("This is not bad outcome.", KeyPhrases());

        score.Should().BeGreaterThan(0.5m);
    }

    [Fact]
    public void AnalyzeSentiment_Should_BeCaseInsensitive_When_KeywordsDifferCase()
    {
        var a = _sut.AnalyzeSentiment("GOOD news.", KeyPhrases());
        var b = _sut.AnalyzeSentiment("good news.", KeyPhrases());

        a.Should().Be(b);
    }

    [Fact]
    public void AnalyzeSentiment_Should_HandlePunctuation_When_WordsSeparated()
    {
        var score = _sut.AnalyzeSentiment("Good!!! Recovery???", KeyPhrases());

        score.Should().BeGreaterThan(0.5m);
    }
}
