using PositiveNews.Domain.Entities;

namespace PositiveNews.Application.Abstractions.Persistence.Repositories.Write;

public interface ITopicWriteRepository
{
    void Add(Topic topic);
}
