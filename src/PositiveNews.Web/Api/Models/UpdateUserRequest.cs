namespace PositiveNews.Web.Api.Models;

public sealed class UpdateUserRequest
{
    public bool IsActive { get; init; }
    public bool EmailConfirmed { get; init; }
    public string? Reason { get; init; }
    public string? Note { get; init; }
}