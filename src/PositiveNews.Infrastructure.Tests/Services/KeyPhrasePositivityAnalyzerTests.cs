using FluentAssertions;
using PositiveNews.Application.DTOs.Ingestion;
using PositiveNews.Infrastructure.Services;

namespace PositiveNews.Infrastructure.Tests.Services;

public class KeyPhrasePositivityAnalyzerTests
{
    private readonly KeyPhrasePositivityAnalyzer _sut = new();

    private static PositivityAnalizerKeyPhrases KeyPhrases() => PositivityAnalyzerTestLexicon.Create(
        positiveWords: new HashSet<string>(["good", "recovery"]),
        negativeWords: new HashSet<string>(["bad", "crisis", "harm", "deaths"]),
        positivePhrases: new HashSet<string>(["breakthrough"]),
        negativePhrases: new HashSet<string>(["bad news"]),
        negationWords: new HashSet<string>(["not"]),
        intensifierWords: new HashSet<string>(["very"]),
        mitigationWords: new HashSet<string>(["zero"]),
        mitigationPhrases: new HashSet<string>(["zero deaths"]),
        ledeCharCount: 80);

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

    [Fact]
    public void AnalyzeSentiment_Should_SuppressNegativeCue_When_MitigationPhrasePresent()
    {
        var mitigated = _sut.AnalyzeSentiment("Researchers reported zero deaths in the cohort.", KeyPhrases());
        var negative = _sut.AnalyzeSentiment("Researchers reported many deaths in the cohort.", KeyPhrases());

        mitigated.Should().BeGreaterThan(negative);
        mitigated.Should().Be(0.5000m);
    }

    [Fact]
    public void AnalyzeSentiment_Should_WeightLedeHigher_When_LongBodyIsNegative()
    {
        var positiveLede = "A very good recovery and breakthrough for the community. ";
        var negativeTail = string.Join(' ', Enumerable.Repeat("bad crisis harm", 40));
        var score = _sut.AnalyzeSentiment(positiveLede + negativeTail, KeyPhrases());

        var tailOnly = _sut.AnalyzeSentiment(negativeTail, KeyPhrases());

        score.Should().BeGreaterThan(tailOnly);
        score.Should().BeGreaterThan(0.5m * tailOnly);
    }

    [Fact]
    public void AnalyzeSentiment_Should_BlendTitle_When_TitleProvided()
    {
        var body = string.Join(' ', Enumerable.Repeat("bad crisis harm", 30));
        var withoutTitle = _sut.AnalyzeSentiment(body, KeyPhrases());
        var withTitle = _sut.AnalyzeSentiment(body, KeyPhrases(), "A very good recovery breakthrough");

        withTitle.Should().BeGreaterThan(withoutTitle);
    }
}
