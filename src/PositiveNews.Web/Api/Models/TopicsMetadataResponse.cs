namespace PositiveNews.Web.Api.Models;

public sealed class TopicsMetadataResponse
{
    public IReadOnlyList<string> TopicNames { get; init; } = Array.Empty<string>();
}
