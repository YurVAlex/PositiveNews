namespace PositiveNews.Web.Models;

public class TopicsPartialViewModel
{
    public List<string> Topics { get; set; } = new();

    // Currently selected topic (for highlighting)
    public string? SelectedTopic { get; set; }
}
