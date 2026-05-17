namespace PositiveNews.Application.Interfaces;

using PositiveNews.Application.DTOs;

/// <summary>
/// Builds an in-memory <see cref="TopicLookup"/> from database topic snapshots.
/// </summary>
public interface ITopicLookupBuilder
{
    /// <summary>
    /// Indexes topics by name, slug fragments, and parent/child relationships for fast resolution.
    /// </summary>
    /// <param name="topics">All topic rows from persistence.</param>
    /// <returns>Immutable lookup structure used during ingestion.</returns>
    TopicLookup Build(IReadOnlyList<TopicSnapshot> topics);
}
