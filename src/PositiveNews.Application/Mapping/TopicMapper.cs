using System.Linq;
using PositiveNews.Application.DTOs;
using PositiveNews.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace PositiveNews.Application.Mapping;

/// <summary>
/// Mapperly projections between <see cref="Topic"/> entities and ingestion snapshots.
/// </summary>
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class TopicMapper
{
    /// <summary>
    /// Maps a topic entity to an immutable snapshot used when building lookups.
    /// </summary>
    public static partial TopicSnapshot ToTopicSnapshot(this Topic source);

    /// <summary>
    /// EF-safe projection of topics into snapshots.
    /// </summary>
    public static partial IQueryable<TopicSnapshot> ProjectToTopicSnapshot(this IQueryable<Topic> query);
}
