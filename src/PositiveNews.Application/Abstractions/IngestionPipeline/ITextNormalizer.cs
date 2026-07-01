namespace PositiveNews.Application.Abstractions.IngestionPipeline;

/// <summary>
/// Performs Unicode and whitespace normalization on feed text fields.
/// </summary>
public interface ITextNormalizer
{
    /// <summary>
    /// Normalizes article HTML content for consistent downstream parsing.
    /// </summary>
    /// <param name="htmlContent">Raw HTML string.</param>
    /// <returns>Normalized HTML.</returns>
    string NormalizeContent(string htmlContent);

    /// <summary>
    /// Normalizes RSS description or summary text.
    /// </summary>
    /// <param name="description">Description string.</param>
    /// <returns>Normalized description.</returns>
    string NormalizeDescription(string description);

    /// <summary>
    /// Normalizes article titles for deduplication and display.
    /// </summary>
    /// <param name="title">Title string.</param>
    /// <returns>Normalized title.</returns>
    string NormalizeTitle(string title);
}
