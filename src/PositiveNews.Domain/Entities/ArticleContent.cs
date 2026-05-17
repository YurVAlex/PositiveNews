namespace PositiveNews.Domain.Entities;

/// <summary>
/// Holds raw and sanitized HTML body for an article. Shares its primary key with <see cref="ArticleMetadata"/> (one-to-one).
/// </summary>
public class ArticleContent
{
    /// <remarks>Used by EF Core when hydrating entities from the database.</remarks>
    private ArticleContent() { }

    /// <summary>Shared PK with ArticleMetadata (1-to-1).</summary>
    public long Id { get; private set; }

    /// <summary>Original HTML or text body from the feed before sanitization.</summary>
    public string? ContentRaw { get; private set; }

    /// <summary>Sanitized HTML safe for display.</summary>
    public string? ContentClean { get; private set; }

    /// <summary>Owning article metadata row.</summary>
    public ArticleMetadata Metadata { get; private set; } = null!;

    /// <summary>
    /// Creates a new content row with optional raw and cleaned bodies.
    /// </summary>
    public static ArticleContent Create(string? contentRaw, string? contentClean)
    {
        return new ArticleContent
        {
            ContentRaw = contentRaw,
            ContentClean = contentClean
        };
    }

    /// <summary>
    /// Replaces stored raw and cleaned content (e.g. after re-ingestion or moderation).
    /// </summary>
    public void UpdateContent(string? contentRaw, string? contentClean)
    {
        ContentRaw = contentRaw;
        ContentClean = contentClean;
    }
}
