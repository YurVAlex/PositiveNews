using PositiveNews.Application.DTOs;
using PositiveNews.Application.Interfaces;

namespace PositiveNews.Application.Services.Ingestion;

/// <summary>
/// Builds slug-word indexes and parent links used during topic normalization.
/// </summary>
internal sealed class TopicLookupBuilder : ITopicLookupBuilder
{
    /// <summary>
    /// Indexes topics by canonical name, slug tokens, and inferred parent relationships.
    /// </summary>
    /// <param name="topics">All topics returned from persistence.</param>
    /// <returns>Immutable lookup shared across one ingestion cycle.</returns>
    public TopicLookup Build(IReadOnlyList<TopicSnapshot> topics)
    {
        var byName = new Dictionary<string, TopicSnapshot>(StringComparer.OrdinalIgnoreCase);
        var bySlugWord = new Dictionary<string, List<TopicSnapshot>>(StringComparer.OrdinalIgnoreCase);
        var childToParent = new Dictionary<string, List<TopicSnapshot>>(StringComparer.OrdinalIgnoreCase);

        foreach (var topic in topics)
        {
            byName[topic.Name] = topic;

            var slugWords = topic.Slug?
                .Split([',', ';', ' ', '|'], StringSplitOptions.RemoveEmptyEntries)
                .Select(w => w.Trim().ToLowerInvariant()) ?? [];

            foreach (var word in slugWords)
            {
                if (!bySlugWord.TryGetValue(word, out var list1))
                {
                    list1 = [];
                    bySlugWord[word] = list1;
                }
                list1.Add(topic);

                if (!childToParent.TryGetValue(word, out var list2))
                {
                    list2 = [];
                    childToParent[word] = list2;
                }
                list2.Add(topic);
            }
        }

        return new TopicLookup(
            byName,
            bySlugWord.ToDictionary(kv => kv.Key, kv => (IReadOnlyList<TopicSnapshot>)kv.Value, StringComparer.OrdinalIgnoreCase),
            childToParent.ToDictionary(kv => kv.Key, kv => (IReadOnlyList<TopicSnapshot>)kv.Value, StringComparer.OrdinalIgnoreCase));
    }
}
