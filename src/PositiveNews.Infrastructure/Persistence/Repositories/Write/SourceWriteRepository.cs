using PositiveNews.Application.Abstractions.Persistence.Repositories.Write;
using PositiveNews.Domain.Entities;
using PositiveNews.Infrastructure.Persistence;

namespace PositiveNews.Infrastructure.Persistence.Repositories.Write;

/// <inheritdoc />
internal sealed class SourceWriteRepository(AppDbContext db) : ISourceWriteRepository
{
    /// <inheritdoc />
    public void Add(Source source) => db.Sources.Add(source);
}
