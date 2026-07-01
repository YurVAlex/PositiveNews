using System.Linq;
using PositiveNews.Application.DTOs.Ingestion;
using PositiveNews.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace PositiveNews.Application.Mapping;

/// <summary>
/// Mapperly projections between <see cref="Source"/> entities and ingestion snapshots.
/// </summary>
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class SourceMapper
{
    /// <summary>
    /// Maps an EF-tracked source row to an immutable ingestion snapshot.
    /// </summary>
    public static partial IngestionSourceSnapshot ToIngestionSourceSnapshot(this Source source);

    /// <summary>
    /// EF-safe projection of sources into ingestion snapshots.
    /// </summary>
    public static partial IQueryable<IngestionSourceSnapshot> ProjectToIngestionSourceSnapshot(this IQueryable<Source> query);
}
