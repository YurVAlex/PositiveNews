using Microsoft.Extensions.Options;
using PositiveNews.Application.DTOs;
using PositiveNews.Application.Interfaces;
using PositiveNews.Infrastructure.Configuration;

namespace PositiveNews.Infrastructure.Services;

/// <summary>
/// Materializes <see cref="IngestionSettingsConfig"/> into immutable snapshots used during ingestion.
/// </summary>
public class IngestionSettingsProvider : IIngestionSettingsProvider
{
    private static HashSet<string> NormalizePhraseSet(IEnumerable<string> phrases)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var p in phrases)
        {
            var normalized = string.Join(' ', p.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
            if (normalized.Length > 0)
                set.Add(normalized);
        }

        return set;
    }

    private readonly IngestionSettingsConfig _config;

    /// <summary>
    /// Initializes a new instance of the <see cref="IngestionSettingsProvider"/> class.
    /// </summary>
    /// <param name="options">Bound <c>IngestionSettings</c> configuration.</param>
    public IngestionSettingsProvider(IOptions<IngestionSettingsConfig> options)
    {
        _config = options.Value;
    }

    /// <inheritdoc />
    public IngestionSettingsSnapshot GetCurrentSettings()
    {
        var pa = _config.Common.PositivityAnalizerKeyPhrases;
        var titleWeight = (decimal)Math.Clamp(pa.TitleWeight, 0.0, 1.0);
        var ledeWeight = (decimal)Math.Clamp(pa.LedeWeight, 0.0, 1.0);
        var bodyWeight = (decimal)Math.Clamp(pa.BodyWeight, 0.0, 1.0);
        var segmentTotal = ledeWeight + bodyWeight;
        if (segmentTotal <= 0m)
        {
            ledeWeight = 0.35m;
            bodyWeight = 0.50m;
            segmentTotal = ledeWeight + bodyWeight;
        }

        ledeWeight /= segmentTotal;
        bodyWeight /= segmentTotal;

        var positivity = new PositivityAnalizerKeyPhrases(
            PositiveWords: new HashSet<string>(pa.PositiveWords, StringComparer.OrdinalIgnoreCase),
            NegativeWords: new HashSet<string>(pa.NegativeWords, StringComparer.OrdinalIgnoreCase),
            PositivePhrases: NormalizePhraseSet(pa.PositivePhrases),
            NegativePhrases: NormalizePhraseSet(pa.NegativePhrases),
            NegationWords: new HashSet<string>(pa.NegationWords, StringComparer.OrdinalIgnoreCase),
            IntensifierWords: new HashSet<string>(pa.IntensifierWords, StringComparer.OrdinalIgnoreCase),
            NegationLookbackTokens: Math.Clamp(pa.NegationLookbackTokens, 1, 12),
            IntensifierLookbackTokens: Math.Clamp(pa.IntensifierLookbackTokens, 1, 8),
            IntensifierMultiplier: (decimal)Math.Clamp(pa.IntensifierMultiplier, 1.0, 3.0),
            PhrasePolarityWeight: (decimal)Math.Clamp(pa.PhrasePolarityWeight, 0.5, 10.0),
            MitigationWords: new HashSet<string>(pa.MitigationWords, StringComparer.OrdinalIgnoreCase),
            MitigationPhrases: NormalizePhraseSet(pa.MitigationPhrases),
            MitigationLookbackTokens: Math.Clamp(pa.MitigationLookbackTokens, 1, 8),
            TitleWeight: titleWeight,
            LedeWeight: ledeWeight,
            BodyWeight: bodyWeight,
            LedeCharCount: Math.Clamp(pa.LedeCharCount, 100, 4000));

        var cleaner = new CleanerRules(
            StopProcessingPatterns: _config.Common.CleanerRules.StopProcessingPatterns,
            RemoveNodePatterns: _config.Common.CleanerRules.RemoveNodePatterns,
            RemoveDivClassPatterns: _config.Common.CleanerRules.RemoveDivClassPatterns,
            ShouldRemoveParagraphPatterns: _config.Common.CleanerRules.ShouldRemoveParagraphPatterns,
            AllowedTags: new HashSet<string>(_config.Common.CleanerRules.AllowedTags, StringComparer.OrdinalIgnoreCase),
            AttributesToRemove: new HashSet<string>(_config.Common.CleanerRules.AttributesToRemove, StringComparer.OrdinalIgnoreCase));

        var validation = new FeedItemValidationRules(
            InvalidAuthors: new HashSet<string>(_config.Common.FeedItemValidationRules.InvalidAuthors, StringComparer.OrdinalIgnoreCase),
            InvalidLinkContains: _config.Common.FeedItemValidationRules.InvalidLinkContains);

        var sources = _config.Sources.ToDictionary(
            kvp => kvp.Key,
            kvp => new SourceIngestionRules(kvp.Value.UrlContains, kvp.Value.DefaultTopics),
            StringComparer.OrdinalIgnoreCase);

        return new IngestionSettingsSnapshot(positivity, cleaner, validation, sources);
    }
}
