namespace PositiveNews.Domain.Entities;

public class ArticleContent
{
    // For EF Core materialization
    private ArticleContent() { }

    /// <summary>Shared PK with ArticleMetadata (1-to-1).</summary>
    public long Id { get; private set; }
    public string? ContentRaw { get; private set; }
    public string? ContentClean { get; private set; }

    // Navigation
    public ArticleMetadata Metadata { get; private set; } = null!;

    public static ArticleContent Create(string? contentRaw, string? contentClean)
    {
        return new ArticleContent
        {
            ContentRaw = contentRaw,
            ContentClean = contentClean
        };
    }

    public void UpdateContent(string? contentRaw, string? contentClean)
    {
        ContentRaw = contentRaw;
        ContentClean = contentClean;
    }
}
