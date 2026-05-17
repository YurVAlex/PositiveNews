using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using PositiveNews.Application.DTOs;
using PositiveNews.Application.Interfaces;

namespace PositiveNews.Infrastructure.Services;

/// <summary>
/// Lexicon-based sentiment: multi-word phrase matching (longest-first, non-overlapping),
/// Unicode word tokens, negation windows (odd number of negation cues flips polarity),
/// intensifier compounding, contraction expansion, and tanh normalization to (0,1) around 0.5.
/// This is a configurable rule-based model (not an external ML or LLM).
/// </summary>
public class KeyPhrasePositivityAnalyzer : IPositivityAnalyzer
{
    private static readonly Regex Whitespace = new(@"\s+", RegexOptions.Compiled);
    private static readonly Regex WordToken = new(@"[\p{L}\p{M}\p{N}]+", RegexOptions.Compiled);

    private static readonly (Regex Pattern, string Replacement)[] ContractionReplacements =
    [
        (new Regex(@"(?i)\bdon't\b", RegexOptions.Compiled), "do not"),
        (new Regex(@"(?i)\bwon't\b", RegexOptions.Compiled), "will not"),
        (new Regex(@"(?i)\bcan't\b", RegexOptions.Compiled), "can not"),
        (new Regex(@"(?i)\bisn't\b", RegexOptions.Compiled), "is not"),
        (new Regex(@"(?i)\baren't\b", RegexOptions.Compiled), "are not"),
        (new Regex(@"(?i)\bwasn't\b", RegexOptions.Compiled), "was not"),
        (new Regex(@"(?i)\bweren't\b", RegexOptions.Compiled), "were not"),
        (new Regex(@"(?i)\bhaven't\b", RegexOptions.Compiled), "have not"),
        (new Regex(@"(?i)\bhasn't\b", RegexOptions.Compiled), "has not"),
        (new Regex(@"(?i)\bhadn't\b", RegexOptions.Compiled), "had not"),
        (new Regex(@"(?i)\bdidn't\b", RegexOptions.Compiled), "did not"),
        (new Regex(@"(?i)\bwouldn't\b", RegexOptions.Compiled), "would not"),
        (new Regex(@"(?i)\bcouldn't\b", RegexOptions.Compiled), "could not"),
        (new Regex(@"(?i)\bshouldn't\b", RegexOptions.Compiled), "should not"),
        (new Regex(@"(?i)\bdoesn't\b", RegexOptions.Compiled), "does not"),
    ];

    /// <inheritdoc />
    public decimal AnalyzeSentiment(string? plainTextContent, PositivityAnalizerKeyPhrases keyPhrases)
    {
        if (string.IsNullOrWhiteSpace(plainTextContent))
            return 0.5000m;

        var norm = NormalizeText(plainTextContent);
        if (norm.Length == 0)
            return 0.5000m;

        var covered = new bool[norm.Length];
        decimal netPolarity = 0m;

        netPolarity += ScorePhrases(norm, covered, keyPhrases);

        var matches = WordToken.Matches(norm);
        for (var i = 0; i < matches.Count; i++)
        {
            var m = matches[i];
            if (Overlaps(m.Index, m.Length, covered))
                continue;

            var word = m.Value.ToLowerInvariant();
            if (word.Length == 0)
                continue;

            var isPositive = keyPhrases.PositiveWords.Contains(word);
            var isNegative = keyPhrases.NegativeWords.Contains(word);
            if (isPositive == isNegative)
                continue;

            var negations = CountCueTokens(matches, i, keyPhrases.NegationWords, keyPhrases.NegationLookbackTokens);
            var flipped = (negations & 1) == 1;

            var intensifiers = CountCueTokens(matches, i, keyPhrases.IntensifierWords, keyPhrases.IntensifierLookbackTokens);
            var weight = BaseWordWeight(intensifiers, keyPhrases.IntensifierMultiplier);

            if (isPositive)
                netPolarity += flipped ? -weight : weight;
            else
                netPolarity += flipped ? weight : -weight;
        }

        if (netPolarity == 0m && !Array.Exists(covered, static x => x))
            return 0.5000m;

        return NormalizeNetToScore(netPolarity, norm.Length);
    }

    private static decimal ScorePhrases(
        string norm,
        bool[] covered,
        PositivityAnalizerKeyPhrases keyPhrases)
    {
        var phraseEntries = new List<(string Text, int Sign)>(keyPhrases.PositivePhrases.Count + keyPhrases.NegativePhrases.Count);
        foreach (var p in keyPhrases.PositivePhrases)
        {
            if (p.Length > 0)
                phraseEntries.Add((p, 1));
        }

        foreach (var p in keyPhrases.NegativePhrases)
        {
            if (p.Length > 0)
                phraseEntries.Add((p, -1));
        }

        phraseEntries.Sort(static (a, b) => b.Text.Length.CompareTo(a.Text.Length));

        decimal net = 0m;
        foreach (var (text, sign) in phraseEntries)
        {
            var pattern = BuildPhraseRegex(text);
            if (pattern == null)
                continue;

            foreach (Match match in pattern.Matches(norm))
            {
                if (Overlaps(match.Index, match.Length, covered))
                    continue;

                net += keyPhrases.PhrasePolarityWeight * sign;
                MarkCovered(covered, match.Index, match.Length);
            }
        }

        return net;
    }

    private static Regex? BuildPhraseRegex(string phrase)
    {
        var parts = phrase.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0)
            return null;

        var inner = string.Join(@"\s+", parts.Select(Regex.Escape));
        return new Regex(@"(?i)\b" + inner + @"\b", RegexOptions.Compiled);
    }

    private static int CountCueTokens(
        MatchCollection allWords,
        int index,
        IReadOnlySet<string> cues,
        int lookbackTokens)
    {
        if (cues.Count == 0 || lookbackTokens <= 0)
            return 0;

        var start = Math.Max(0, index - lookbackTokens);
        var count = 0;
        for (var j = start; j < index; j++)
        {
            var w = allWords[j].Value.ToLowerInvariant();
            if (cues.Contains(w))
                count++;
        }

        return count;
    }

    private static decimal BaseWordWeight(int intensifierCount, decimal intensifierMultiplier)
    {
        if (intensifierCount <= 0 || intensifierMultiplier <= 1m)
            return 1m;

        var factor = Math.Pow((double)intensifierMultiplier, intensifierCount);
        return (decimal)factor;
    }

    private static bool Overlaps(int index, int length, bool[] covered)
    {
        var end = Math.Min(covered.Length, index + length);
        for (var i = index; i < end; i++)
        {
            if (covered[i])
                return true;
        }

        return false;
    }

    private static void MarkCovered(bool[] covered, int index, int length)
    {
        var end = Math.Min(covered.Length, index + length);
        for (var i = index; i < end; i++)
            covered[i] = true;
    }

    private static string NormalizeText(string text)
    {
        var span = text.AsSpan();
        var sb = new StringBuilder(span.Length);
        foreach (var ch in span)
        {
            if (char.GetUnicodeCategory(ch) == UnicodeCategory.Format)
                continue;
            sb.Append(char.IsWhiteSpace(ch) ? ' ' : ch);
        }

        var collapsed = Whitespace.Replace(sb.ToString(), " ").Trim();
        foreach (var (rx, repl) in ContractionReplacements)
            collapsed = rx.Replace(collapsed, repl);

        return Whitespace.Replace(collapsed, " ").Trim();
    }

    /// <summary>
    /// Maps accumulated polarity to [0,1] with midpoint 0.5; divisor scales typical article-length evidence.
    /// </summary>
    private static decimal NormalizeNetToScore(decimal netPolarity, int normalizedCharLength)
    {
        var lengthScale = 1.0 + Math.Log(1.0 + Math.Max(0, normalizedCharLength) / 800.0);
        var x = (double)netPolarity / (10.0 * lengthScale);
        var y = 0.5 + 0.5 * Math.Tanh(x);
        if (y <= 0.0)
            return 0m;
        if (y >= 1.0)
            return 1m;
        return Math.Round((decimal)y, 3);
    }
}
