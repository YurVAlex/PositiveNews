using FluentAssertions;
using Microsoft.Extensions.Options;
using PositiveNews.Infrastructure.Configuration;
using PositiveNews.Infrastructure.Services;

namespace PositiveNews.Infrastructure.Tests.Services;

public class IngestionSettingsProviderTests
{
    [Fact]
    public void GetCurrentSettings_Should_BuildSnapshot_When_ConfigMinimal()
    {
        var config = new IngestionSettingsConfig
        {
            Common = new CommonIngestionConfig
            {
                PositivityAnalizerKeyPhrases = new PositivityAnalizerKeyPhrasesConfig
                {
                    PositiveWords = ["good"],
                    NegativeWords = ["bad"],
                    NegationLookbackTokens = 99,
                    IntensifierLookbackTokens = 99,
                    IntensifierMultiplier = 10,
                    PhrasePolarityWeight = 100
                },
                CleanerRules = new CleanerRulesConfig
                {
                    AllowedTags = ["p"],
                    AttributesToRemove = ["style"]
                },
                FeedItemValidationRules = new FeedItemValidationRulesConfig
                {
                    InvalidAuthors = ["spam"],
                    InvalidLinkContains = ["photojournal"]
                }
            }
        };
        var sut = new IngestionSettingsProvider(Options.Create(config));

        var snapshot = sut.GetCurrentSettings();

        snapshot.PositivityAnalizerKeyPhrases.PositiveWords.Should().Contain("good");
        snapshot.PositivityAnalizerKeyPhrases.NegationLookbackTokens.Should().Be(12);
        snapshot.PositivityAnalizerKeyPhrases.IntensifierLookbackTokens.Should().Be(8);
        snapshot.PositivityAnalizerKeyPhrases.IntensifierMultiplier.Should().Be(3m);
        snapshot.PositivityAnalizerKeyPhrases.PhrasePolarityWeight.Should().Be(10m);
        snapshot.CleanerRules.AllowedTags.Should().Contain("p");
        snapshot.FeedItemValidationRules.InvalidAuthors.Should().Contain("spam");
        snapshot.FeedItemValidationRules.InvalidLinkContains.Should().Contain("photojournal");
    }

    [Fact]
    public void GetCurrentSettings_Should_NormalizePhraseWhitespace_When_PhrasesHaveExtraSpaces()
    {
        var config = new IngestionSettingsConfig
        {
            Common = new CommonIngestionConfig
            {
                PositivityAnalizerKeyPhrases = new PositivityAnalizerKeyPhrasesConfig
                {
                    PositivePhrases = ["  hello   world  "],
                    NegativeWords = [],
                    PositiveWords = [],
                    NegationWords = [],
                    IntensifierWords = [],
                    NegativePhrases = []
                },
                CleanerRules = new CleanerRulesConfig(),
                FeedItemValidationRules = new FeedItemValidationRulesConfig()
            }
        };
        var sut = new IngestionSettingsProvider(Options.Create(config));

        var snapshot = sut.GetCurrentSettings();

        snapshot.PositivityAnalizerKeyPhrases.PositivePhrases.Should().Contain("hello world");
    }
}
