using PositiveNews.Application.DTOs.Ingestion;

namespace PositiveNews.Application.Abstractions.IngestionPipeline;

/// <summary>
/// Scores plain-text article content for positivity using configurable lexicons.
/// </summary>
public interface IPositivityAnalyzer
{
    /// <summary>
    /// Computes a sentiment score from token-level and phrase-level signals.
    /// </summary>
    /// <param name="plainTextContent">Normalized plain text (may be empty).</param>
    /// <param name="keyPhrases">Positive/negative word lists and tuning parameters.</param>
    /// <param name="title">Optional article title, weighted separately when present.</param>
    /// <returns>Numeric positivity score for persistence and UI.</returns>
    decimal AnalyzeSentiment(
        string? plainTextContent,
        PositivityAnalizerKeyPhrases keyPhrases,
        string? title = null);
}
