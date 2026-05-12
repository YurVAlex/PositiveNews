using PositiveNews.Application.Abstractions.Persistence.Repositories.Write;
using PositiveNews.Domain.Entities;
using PositiveNews.Infrastructure.Persistence;

namespace PositiveNews.Infrastructure.Persistence.Repositories.Write;

/// <inheritdoc />
internal sealed class TopicWriteRepository(AppDbContext db) : ITopicWriteRepository
{
    /// <inheritdoc />
    public void Add(Topic topic) => db.Topics.Add(topic);
}
