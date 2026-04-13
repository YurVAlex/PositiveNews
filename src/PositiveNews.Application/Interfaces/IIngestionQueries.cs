using PositiveNews.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PositiveNews.Application.Interfaces;

public interface IIngestionQueries
{
    Task<TopicLookup> GetTopicLookupAsync(CancellationToken cancellationToken = default);
    Task<List<Source>> GetActiveSourcesAsync(CancellationToken cancellationToken = default);
    Task<bool> ArticleExistsAsync(string? externalId, string? url, string title, CancellationToken cancellationToken = default);
}