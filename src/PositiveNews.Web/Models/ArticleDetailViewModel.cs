namespace PositiveNews.Web.Models
{
    public class ArticleDetailViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string SourceName { get; set; } = string.Empty;
        public string? Author { get; set; }
        public DateTime PublishedAt { get; set; }
        public string? ContentHtml { get; set; }
    }
}
