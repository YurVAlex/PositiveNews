namespace PositiveNews.Application.DTOs;

public sealed record TopicLookup(
    IReadOnlyDictionary<string, TopicSnapshot> ByName,
    IReadOnlyDictionary<string, IReadOnlyList<TopicSnapshot>> BySlugWord,
    IReadOnlyDictionary<string, IReadOnlyList<TopicSnapshot>> ChildToParentTopics);
