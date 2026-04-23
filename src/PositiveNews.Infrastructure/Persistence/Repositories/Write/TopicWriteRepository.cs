using PositiveNews.Application.Abstractions.Persistence.Repositories.Write;
using PositiveNews.Domain.Entities;
using PositiveNews.Infrastructure.Persistence;

namespace PositiveNews.Infrastructure.Persistence.Repositories.Write;

internal sealed class TopicWriteRepository(AppDbContext db) : ITopicWriteRepository
{
    public void Add(Topic topic) => db.Topics.Add(topic);
}
