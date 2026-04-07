// Add to TopicLookup class
using PositiveNews.Domain.Entities;

public class TopicLookup
{
    public Dictionary<string, Topic> ByName { get; set; } = new();
    public Dictionary<string, List<Topic>> BySlugWord { get; set; } = new();
    public Dictionary<string, List<Topic>> ChildToParentTopics { get; set; } = new(); // NEW

    public static TopicLookup Build(List<Topic> topics)
    {
        var lookup = new TopicLookup();

        foreach (var topic in topics)
        {
            // Existing lookups
            lookup.ByName[topic.Name] = topic;

            var slugWords = topic.Slug?
                .Split(new[] { ',', ';', ' ', '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(w => w.Trim().ToLowerInvariant()) ?? Array.Empty<string>();

            foreach (var word in slugWords)
            {
                if (!lookup.BySlugWord.ContainsKey(word))
                    lookup.BySlugWord[word] = new List<Topic>();
                lookup.BySlugWord[word].Add(topic);
            }

            // NEW: Build reverse mapping - each slug word maps back to this topic as parent
            foreach (var word in slugWords)
            {
                if (!lookup.ChildToParentTopics.ContainsKey(word))
                    lookup.ChildToParentTopics[word] = new List<Topic>();
                lookup.ChildToParentTopics[word].Add(topic);
            }
        }

        return lookup;
    }
}