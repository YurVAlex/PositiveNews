using PositiveNews.Application.DTOs;

namespace PositiveNews.Application.Abstractions.Persistence.Repositories.Read;

public interface ISourceReadRepository
{
    Task<IReadOnlyList<IngestionSourceSnapshot>> GetActiveIngestionSourcesAsync(CancellationToken ct);
}
