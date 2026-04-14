using PositiveNews.Domain.Entities;

namespace PositiveNews.Application.DTOs;

public record TopicLookup
{
    public Dictionary<string, Topic> ByName { get; set; } = [];
    public Dictionary<string, List<Topic>> BySlugWord { get; set; } = [];
    public Dictionary<string, List<Topic>> ChildToParentTopics { get; set; } = [];

    public static TopicLookup Build(IReadOnlyList<Topic> topics)
    {
        var lookup = new TopicLookup();

        foreach (var topic in topics)
        {
            lookup.ByName[topic.Name] = topic;

            var slugWords = topic.Slug?
                .Split([',', ';', ' ', '|'], StringSplitOptions.RemoveEmptyEntries)
                .Select(w => w.Trim().ToLowerInvariant()) ?? [];

            foreach (var word in slugWords)
            {
                if (!lookup.BySlugWord.ContainsKey(word))
                    lookup.BySlugWord[word] = [];
                lookup.BySlugWord[word].Add(topic);
            }

            foreach (var word in slugWords)
            {
                if (!lookup.ChildToParentTopics.ContainsKey(word))
                    lookup.ChildToParentTopics[word] = [];
                lookup.ChildToParentTopics[word].Add(topic);
            }
        }

        return lookup;
    }
}
