using Microsoft.Extensions.Options;
using PositiveNews.Application.DTOs;
using PositiveNews.Application.Interfaces;
using PositiveNews.Infrastructure.Configuration;

namespace PositiveNews.Infrastructure.Services;

public class IngestionSettingsProvider : IIngestionSettingsProvider
{
    private readonly IngestionSettingsConfig _config; //TODO Add public method for changing config via admin panel

    public IngestionSettingsProvider(IOptions<IngestionSettingsConfig> options)
    {
        _config = options.Value;
    }

    public IngestionSettingsSnapshot GetCurrentSettings()
    {
        var positivity = new PositivityAnalizerKeyPhrases(
            PositiveWords: new HashSet<string>(_config.Common.PositivityAnalizerKeyPhrases.PositiveWords, StringComparer.OrdinalIgnoreCase),
            NegativeWords: new HashSet<string>(_config.Common.PositivityAnalizerKeyPhrases.NegativeWords, StringComparer.OrdinalIgnoreCase));

        var cleaner = new CleanerRules(
            StopProcessingPatterns: _config.Common.CleanerRules.StopProcessingPatterns,
            RemoveNodePatterns: _config.Common.CleanerRules.RemoveNodePatterns,
            RemoveDivClassPatterns: _config.Common.CleanerRules.RemoveDivClassPatterns,
            ShouldRemoveParagraphPatterns: _config.Common.CleanerRules.ShouldRemoveParagraphPatterns,
            AllowedTags: new HashSet<string>(_config.Common.CleanerRules.AllowedTags, StringComparer.OrdinalIgnoreCase),
            AttributesToRemove: new HashSet<string>(_config.Common.CleanerRules.AttributesToRemove, StringComparer.OrdinalIgnoreCase));

        var validation = new FeedItemValidationRules(
            InvalidAuthors: new HashSet<string>(_config.Common.FeedItemValidationRules.InvalidAuthors, StringComparer.OrdinalIgnoreCase));

        var sources = _config.Sources.ToDictionary(
            kvp => kvp.Key,
            kvp => new SourceIngestionRules(kvp.Value.UrlContains, kvp.Value.DefaultTopics),
            StringComparer.OrdinalIgnoreCase);

        return new IngestionSettingsSnapshot(positivity, cleaner, validation, sources);
    }
}
