using PositiveNews.Application.Abstractions.Persistence.Repositories.Write;
using PositiveNews.Domain.Entities;
using PositiveNews.Infrastructure.Persistence;

namespace PositiveNews.Infrastructure.Persistence.Repositories.Write;

internal sealed class SourceWriteRepository(AppDbContext db) : ISourceWriteRepository
{
    public void Add(Source source) => db.Sources.Add(source);
}
