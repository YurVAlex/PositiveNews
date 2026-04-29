using PositiveNews.Application.DTOs;
using PositiveNews.Application.Interfaces;

namespace PositiveNews.Infrastructure.Services;

public class KeyPhrasePositivityAnalyzer : IPositivityAnalyzer
{
    public decimal AnalyzeSentiment(string? plainTextContent, CommonIngestionRules rules)
    {
        if (string.IsNullOrWhiteSpace(plainTextContent))
            return 0.5000m;

        var words = plainTextContent.Split(
            [' ', '.', ',', ';', '!', '?', '\n', '\r', '"', '\''],
            StringSplitOptions.RemoveEmptyEntries);

        int posCount = 0;
        int negCount = 0;

        foreach (var word in words)
        {
            if (rules.PositiveWords.Contains(word)) posCount++;
            if (rules.NegativeWords.Contains(word)) negCount++;
        }

        int totalScored = posCount + negCount;

        if (totalScored == 0)
            return 0.5000m;

        decimal score = (decimal)posCount / totalScored;

        return Math.Round(score, 3);
    }
}
