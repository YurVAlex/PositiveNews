using PositiveNews.Domain.Entities;

namespace PositiveNews.Application.Abstractions.Persistence.Repositories.Write;

/// <summary>
/// Writes topic taxonomy rows.
/// </summary>
public interface ITopicWriteRepository
{
    /// <summary>
    /// Stages a new topic for insertion on commit.
    /// </summary>
    /// <param name="topic">Topic entity.</param>
    void Add(Topic topic);
}
