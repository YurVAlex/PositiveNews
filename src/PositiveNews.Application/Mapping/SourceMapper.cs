using System.Linq;
using PositiveNews.Application.DTOs;
using PositiveNews.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace PositiveNews.Application.Mapping;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class SourceMapper
{
    public static partial IngestionSourceSnapshot ToIngestionSourceSnapshot(this Source source);

    public static partial IQueryable<IngestionSourceSnapshot> ProjectToIngestionSourceSnapshot(this IQueryable<Source> query);
}
