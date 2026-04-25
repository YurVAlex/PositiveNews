using System.Linq;
using PositiveNews.Application.DTOs;
using PositiveNews.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace PositiveNews.Application.Mapping;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class TopicMapper
{
    public static partial TopicSnapshot ToTopicSnapshot(this Topic source);

    public static partial IQueryable<TopicSnapshot> ProjectToTopicSnapshot(this IQueryable<Topic> query);
}
