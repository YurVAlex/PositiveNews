namespace PositiveNews.Web.Models;

public class FeedIndexViewModel
{
    public List<ArticlePreviewViewModel> Articles { get; set; } = new();

    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }

    // Active topic filter (null = no filter)
    public string? SelectedTopic { get; set; }
}
