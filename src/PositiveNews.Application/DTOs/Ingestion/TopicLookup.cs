namespace PositiveNews.Application.DTOs.Ingestion;

/// <summary>
/// Precomputed indexes for resolving raw topic strings to database topics during ingestion.
/// </summary>
/// <param name="ByName">Exact lookup by canonical topic name.</param>
/// <param name="BySlugWord">Topics reachable by individual slug fragments.</param>
/// <param name="ChildToParentTopics">Maps child slug tokens to parent topic candidates.</param>
public sealed record TopicLookup(
    IReadOnlyDictionary<string, TopicSnapshot> ByName,
    IReadOnlyDictionary<string, IReadOnlyList<TopicSnapshot>> BySlugWord,
    IReadOnlyDictionary<string, IReadOnlyList<TopicSnapshot>> ChildToParentTopics);
