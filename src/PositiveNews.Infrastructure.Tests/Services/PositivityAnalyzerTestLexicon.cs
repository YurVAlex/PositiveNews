using PositiveNews.Application.DTOs.Ingestion;

namespace PositiveNews.Infrastructure.Tests.Services;

internal static class PositivityAnalyzerTestLexicon
{
    private static readonly HashSet<string> Empty = new(StringComparer.OrdinalIgnoreCase);

    public static PositivityAnalizerKeyPhrases Create(
        IReadOnlySet<string>? positiveWords = null,
        IReadOnlySet<string>? negativeWords = null,
        IReadOnlySet<string>? positivePhrases = null,
        IReadOnlySet<string>? negativePhrases = null,
        IReadOnlySet<string>? negationWords = null,
        IReadOnlySet<string>? intensifierWords = null,
        int negationLookbackTokens = 2,
        int intensifierLookbackTokens = 1,
        decimal intensifierMultiplier = 1.5m,
        decimal phrasePolarityWeight = 2m,
        IReadOnlySet<string>? mitigationWords = null,
        IReadOnlySet<string>? mitigationPhrases = null,
        int mitigationLookbackTokens = 4,
        decimal titleWeight = 0.15m,
        decimal ledeWeight = 0.35m,
        decimal bodyWeight = 0.50m,
        int ledeCharCount = 500)
        => new(
            positiveWords ?? Empty,
            negativeWords ?? Empty,
            positivePhrases ?? Empty,
            negativePhrases ?? Empty,
            negationWords ?? Empty,
            intensifierWords ?? Empty,
            negationLookbackTokens,
            intensifierLookbackTokens,
            intensifierMultiplier,
            phrasePolarityWeight,
            mitigationWords ?? Empty,
            mitigationPhrases ?? Empty,
            mitigationLookbackTokens,
            titleWeight,
            ledeWeight,
            bodyWeight,
            ledeCharCount);
}
