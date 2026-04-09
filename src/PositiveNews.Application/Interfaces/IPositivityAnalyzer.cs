using System;
using System.Collections.Generic;
using System.Text;

namespace PositiveNews.Application.Interfaces;

public interface IPositivityAnalyzer
{
    /// <summary>
    /// Analyzes the sentiment of a given HTML or plain text string.
    /// Returns a decimal between 0.0000 and 1.0000.
    /// </summary>
    decimal AnalyzeSentiment(string? htmlContent);
}