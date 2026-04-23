namespace PositiveNews.Application.DTOs;

public sealed record TopicSnapshot(
    int Id,
    string Name,
    string Slug,
    string? Description);
