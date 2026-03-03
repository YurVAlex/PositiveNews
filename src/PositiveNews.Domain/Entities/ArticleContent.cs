namespace PositiveNews.Domain.Entities;

public class ArticleContent
{
    public long Id { get; set; } // Same PK as ArticleMetadata (1-to-1)
    public string? ContentRaw { get; set; }
    public string? ContentClean { get; set; }
    public string? SummaryShort { get; set; }

    // Navigation
    public ArticleMetadata Metadata { get; set; } = null!;
}