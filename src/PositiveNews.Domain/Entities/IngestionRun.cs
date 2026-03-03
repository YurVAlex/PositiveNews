using PositiveNews.Domain.Enums;

namespace PositiveNews.Domain.Entities;

public class IngestionRun
{
    public long Id { get; set; }
    public int SourceId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public IngestionStatus Status { get; set; }
    public int ItemsFetched { get; set; }
    public string? ErrorMessage { get; set; }

    // Navigation
    public Source Source { get; set; } = null!;
}