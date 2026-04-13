using PositiveNews.Application.DTOs;
using PositiveNews.Domain.Entities;
using PositiveNews.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace PositiveNews.Application.Interfaces;

public interface IIngestionCommands
{
    Task<IngestionRun> StartRunAsync(int sourceId, CancellationToken cancellationToken = default);
    Task CompleteRunAsync(IngestionRun run, IngestionStatus status, int itemsFetched, string? errorMessage = null, CancellationToken cancellationToken = default);
    Task SaveArticleWithTopicsAsync(Source source, RssFeedItemDto dto, CancellationToken cancellationToken = default);
}
