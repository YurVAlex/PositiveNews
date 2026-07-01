using FluentAssertions;
using PositiveNews.Application.DTOs.Ingestion;
using PositiveNews.Infrastructure.Services;

namespace PositiveNews.Infrastructure.Tests.Services;

public class KeyPhrasePositivityAnalyzerBenchmarkTests
{
    private const double MinimumSpearmanCorrelation = 0.40;

    private static readonly KeyPhrasePositivityAnalyzer Analyzer = new();
    private static readonly PositivityAnalizerKeyPhrases Lexicon =
        PositivityAnalyzerProductionLexicon.Create();

    public static TheoryData<string, decimal, string?> LabeledSamples
    {
        get
        {
            var data = new TheoryData<string, decimal, string?>();
            foreach (var sample in SentimentBenchmarkSamples.All)
                data.Add(sample.Text, sample.HumanScore, sample.Title);
            return data;
        }
    }

    [Theory]
    [MemberData(nameof(LabeledSamples))]
    public void AnalyzeSentiment_Should_StayInRange_When_SampleScored(string text, decimal humanScore, string? title)
    {
        var machineScore = Analyzer.AnalyzeSentiment(text, Lexicon, title);

        machineScore.Should().BeInRange(0m, 1m);

        if (humanScore >= 0.65m)
            machineScore.Should().BeGreaterThan(0.52m);
        else if (humanScore <= 0.50m)
            machineScore.Should().BeLessThan(0.58m);
    }

    [Fact]
    public void AnalyzeSentiment_Should_CorrelateWithManualLabels_When_BenchmarkSamplesUsed()
    {
        var humanScores = new List<double>();
        var machineScores = new List<double>();

        foreach (var sample in SentimentBenchmarkSamples.All)
        {
            humanScores.Add((double)sample.HumanScore);
            machineScores.Add((double)Analyzer.AnalyzeSentiment(sample.Text, Lexicon, sample.Title));
        }

        SpearmanCorrelation(humanScores, machineScores).Should().BeGreaterThanOrEqualTo(MinimumSpearmanCorrelation);
    }

    [Fact]
    public void AnalyzeSentiment_Should_RankPositiveAboveNegative_When_BenchmarkExtremesCompared()
    {
        var positive = SentimentBenchmarkSamples.All.OrderByDescending(s => s.HumanScore).First();
        var negative = SentimentBenchmarkSamples.All.OrderBy(s => s.HumanScore).First();

        var positiveScore = Analyzer.AnalyzeSentiment(positive.Text, Lexicon, positive.Title);
        var negativeScore = Analyzer.AnalyzeSentiment(negative.Text, Lexicon, negative.Title);

        positiveScore.Should().BeGreaterThan(negativeScore);
    }

    private static double SpearmanCorrelation(IReadOnlyList<double> xs, IReadOnlyList<double> ys)
    {
        if (xs.Count != ys.Count || xs.Count < 2)
            return 0;

        var xRanks = Rank(xs);
        var yRanks = Rank(ys);
        var n = xs.Count;
        var sumD2 = 0.0;

        for (var i = 0; i < n; i++)
        {
            var d = xRanks[i] - yRanks[i];
            sumD2 += d * d;
        }

        return 1.0 - (6.0 * sumD2) / (n * ((n * n) - 1));
    }

    private static double[] Rank(IReadOnlyList<double> values)
    {
        var ordered = values
            .Select((value, index) => (value, index))
            .OrderBy(x => x.value)
            .ToList();

        var ranks = new double[values.Count];
        var i = 0;
        while (i < ordered.Count)
        {
            var j = i;
            while (j + 1 < ordered.Count && ordered[j + 1].value.Equals(ordered[i].value))
                j++;

            var avgRank = (i + j + 2) / 2.0;
            for (var k = i; k <= j; k++)
                ranks[ordered[k].index] = avgRank;

            i = j + 1;
        }

        return ranks;
    }
}
