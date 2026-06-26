namespace PositiveNews.Web.Api.Models;

/// <summary>
/// Request payload for token refresh.
/// </summary>
public sealed class RefreshRequest
{
    /// <summary>
    /// Gets the refresh token string.
    /// </summary>
    public string RefreshToken { get; init; } = string.Empty;
}
