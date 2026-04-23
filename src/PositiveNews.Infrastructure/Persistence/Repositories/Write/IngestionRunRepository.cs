using PositiveNews.Application.Abstractions.Persistence.Repositories.Write;
using PositiveNews.Domain.Entities;
using PositiveNews.Infrastructure.Persistence;

namespace PositiveNews.Infrastructure.Persistence.Repositories.Write;

internal sealed class IngestionRunRepository(AppDbContext db) : IIngestionRunRepository
{
    public void Add(IngestionRun run) => db.IngestionRuns.Add(run);
}
