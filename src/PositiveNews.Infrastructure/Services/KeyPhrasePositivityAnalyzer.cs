using PositiveNews.Application.Interfaces;

namespace PositiveNews.Infrastructure.Services;

public class KeyPhrasePositivityAnalyzer : IPositivityAnalyzer
{
    private static readonly HashSet<string> PositiveWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "good", "great", "excellent", "positive", "happy", "success", "breakthrough",
        "innovative", "uplifting", "joy", "wonderful", "win", "progress", "inspiring",
        "cure", "hero", "solution", "miracle", "triumph", "beautiful", "love"
    };

    private static readonly HashSet<string> NegativeWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "bad", "terrible", "awful", "negative", "sad", "fail", "failure", "crisis",
        "disaster", "tragedy", "loss", "pain", "death", "murder", "war", "crash",
        "devastating", "horrible", "fear", "hate", "violence"
    };

    public decimal AnalyzeSentiment(string? plainTextContent)
    {
        if (string.IsNullOrWhiteSpace(plainTextContent))
            return 0.5000m; // Neutral default

        // 1. Split text into distinct words
        var words = plainTextContent.Split(new[] { ' ', '.', ',', ';', '!', '?', '\n', '\r', '"', '\'' },
                               StringSplitOptions.RemoveEmptyEntries);

        int posCount = 0;
        int negCount = 0;

        foreach (var word in words)
        {
            if (PositiveWords.Contains(word)) posCount++;
            if (NegativeWords.Contains(word)) negCount++;
        }

        int totalScored = posCount + negCount;

        if (totalScored == 0)
            return 0.5000m; // Neutral if no keywords matched

        // 2. Calculate score between 0.0000 and 1.0000
        decimal score = (decimal)posCount / totalScored;

        return Math.Round(score, 3);
    }
}