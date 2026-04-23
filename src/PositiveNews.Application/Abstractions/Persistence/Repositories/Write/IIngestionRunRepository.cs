using PositiveNews.Domain.Entities;

namespace PositiveNews.Application.Abstractions.Persistence.Repositories.Write;

public interface IIngestionRunRepository
{
    void Add(IngestionRun run);
}
