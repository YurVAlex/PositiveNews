using HtmlAgilityPack;
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

    public decimal AnalyzeSentiment(string? htmlContent)
    {
        if (string.IsNullOrWhiteSpace(htmlContent))
            return 0.5000m; // Neutral default

        // 1. Strip HTML tags to analyze purely text
        var doc = new HtmlDocument();
        doc.LoadHtml(htmlContent);
        var plainText = doc.DocumentNode.InnerText; 

        if (string.IsNullOrWhiteSpace(plainText))
            return 0.5000m;

        // 2. Split text into distinct words
        var words = plainText.Split(new[] { ' ', '.', ',', ';', '!', '?', '\n', '\r', '"', '\'' },
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

        // 3. Calculate score between 0.0000 and 1.0000
        decimal score = (decimal)posCount / totalScored;

        return Math.Round(score, 3);
    }
}