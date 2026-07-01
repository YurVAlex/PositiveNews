using PositiveNews.Application.DTOs.Ingestion;
using PositiveNews.Application.Abstractions.IngestionPipeline;

namespace PositiveNews.Infrastructure.Services;

/// <summary>
/// Maps raw RSS category strings to catalog topic names using slug-word overlap against a prebuilt lookup.
/// </summary>
public class TopicNormalizer : ITopicNormalizer
{
    private static readonly string[] CommonWords =
        ["new", "old", "big", "small", "good", "bad", "hot", "cold"];

    /// <inheritdoc />
    public IReadOnlyList<string> NormalizeTopics(IReadOnlyList<string> rawTopics, TopicLookup lookup)
    {
        if (rawTopics == null || rawTopics.Count == 0)
            return [];

        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var raw in rawTopics)
        {
            var words = raw
                .Split([' ', ',', ';', '&', '/', '|', '-'], StringSplitOptions.RemoveEmptyEntries)
                .Select(w => w.Trim().ToLowerInvariant())
                .Where(w => w.Length > 2);

            foreach (var word in words)
            {
                if (lookup.BySlugWord.TryGetValue(word, out var matchedTopics))
                {
                    foreach (var topic in matchedTopics)
                    {
                        if (IsMeaningfulMatch(word))
                            result.Add(topic.Name);
                    }
                }
            }
        }

        return result.ToList();
    }

    private static bool IsMeaningfulMatch(string word)
    {
        if (CommonWords.Contains(word))
            return false;

        if (int.TryParse(word, out _))
            return false;

        return true;
    }
}
