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
        var common = new CommonIngestionRules(
            PositiveWords: new HashSet<string>(_config.Common.PositiveWords, StringComparer.OrdinalIgnoreCase),
            NegativeWords: new HashSet<string>(_config.Common.NegativeWords, StringComparer.OrdinalIgnoreCase),
            StopProcessingPatterns: _config.Common.StopProcessingPatterns,
            RemoveNodePatterns: _config.Common.RemoveNodePatterns,
            RemoveDivClassPatterns: _config.Common.RemoveDivClassPatterns,
            ShouldRemoveParagraphPatterns: _config.Common.ShouldRemoveParagraphPatterns,
            AllowedTags: new HashSet<string>(_config.Common.AllowedTags, StringComparer.OrdinalIgnoreCase),
            AttributesToRemove: new HashSet<string>(_config.Common.AttributesToRemove, StringComparer.OrdinalIgnoreCase));

        var sources = _config.Sources.ToDictionary(
            kvp => kvp.Key,
            kvp => new SourceIngestionRules(kvp.Value.UrlContains, kvp.Value.DefaultTopics),
            StringComparer.OrdinalIgnoreCase);

        return new IngestionSettingsSnapshot(common, sources);
    }
}
