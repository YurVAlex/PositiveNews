using PositiveNews.Domain.Entities;

namespace PositiveNews.Application.Abstractions.Persistence.Repositories.Write;

public interface ISourceWriteRepository
{
    void Add(Source source);
}
