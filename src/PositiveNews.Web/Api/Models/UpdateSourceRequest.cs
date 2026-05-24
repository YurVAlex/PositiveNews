using System.ComponentModel.DataAnnotations;

namespace PositiveNews.Web.Api.Models;

/// <summary>
/// Represents the admin edit payload for a source.
/// </summary>
public sealed class UpdateSourceRequest
{
    [Required]
    public decimal TrustScore { get; init; }

    [Required]
    public bool IsActive { get; init; }

    [Required]
    [StringLength(1024)]
    public string FeedUrl { get; init; } = string.Empty;

    [StringLength(256)]
    public string? Reason { get; init; }

    [StringLength(1024)]
    public string? Note { get; init; }
}
