using PositiveNews.Application.DTOs;

namespace PositiveNews.Application.Interfaces;

public interface IPositivityAnalyzer
{
    decimal AnalyzeSentiment(string? plainTextContent, CommonIngestionRules rules);
}
