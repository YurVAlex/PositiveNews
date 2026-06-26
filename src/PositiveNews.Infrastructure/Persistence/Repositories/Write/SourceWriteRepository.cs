using Microsoft.EntityFrameworkCore;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Write;
using PositiveNews.Domain.Entities;
using PositiveNews.Infrastructure.Persistence;

namespace PositiveNews.Infrastructure.Persistence.Repositories.Write;

/// <inheritdoc />
internal sealed class SourceWriteRepository(AppDbContext db) : ISourceWriteRepository
{
    /// <inheritdoc />
    public void Add(Source source) => db.Sources.Add(source);

    /// <inheritdoc />
    public Task<Source?> GetByIdAsync(int sourceId, CancellationToken ct)
        => db.Sources.FindAsync(new object[] { sourceId }, ct).AsTask();
}
