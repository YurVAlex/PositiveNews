using PositiveNews.Application.DTOs;

namespace PositiveNews.Application.Abstractions.Persistence.Repositories.Read;

public interface ITopicReadRepository
{
    Task<IReadOnlyList<string>> GetTopicNamesAsync(CancellationToken ct);
    Task<IReadOnlyList<TopicSnapshot>> GetAllTopicSnapshotsAsync(CancellationToken ct);
    Task<IReadOnlyDictionary<string, int>> GetTopicIdsByNamesAsync(IReadOnlyCollection<string> names, CancellationToken ct);
}
