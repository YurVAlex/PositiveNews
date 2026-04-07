namespace PositiveNews.Web.Models
{
    public class ArticlePreviewViewModel
    {
        public long Id { get; set; }
        public string SourceName { get; set; } = string.Empty;
        public string? SourceLogoUrl { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Author { get; set; }
        public DateTime PublishedAt { get; set; }
        public string? ImageTag { get; set; }
        public string? SummaryShort { get; set; }

        public List<string> Topics { get; set; } = new();
    }
}
