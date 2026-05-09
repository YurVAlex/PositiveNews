using PositiveNews.Domain.Entities;

namespace PositiveNews.Application.Abstractions.Persistence.Repositories.Write;

public interface IUserWriteRepository
{
    void Add(User user);
}
