using PositiveNews.Application.Features.Auth.Models;
using PositiveNews.Web.Api.Models;
using Riok.Mapperly.Abstractions;

namespace PositiveNews.Web.Api.Mapping;

/// <summary>
/// Mapperly mappings between application auth models and HTTP API response DTOs.
/// </summary>
[Mapper]
public static partial class AuthApiMapper
{
    /// <summary>
    /// Maps an authentication result to the wire-format <see cref="Models.AuthResponse"/>.
    /// </summary>
    /// <param name="source">Domain/application auth payload.</param>
    /// <returns>The API response model.</returns>
    public static partial AuthResponse ToAuthResponse(this AuthResultModel source);

    /// <summary>
    /// Maps a user profile model to the wire-format <see cref="Models.UserProfileResponse"/>.
    /// </summary>
    /// <param name="source">Application-layer user profile.</param>
    /// <returns>The API profile DTO.</returns>
    public static partial UserProfileResponse ToUserProfileResponse(this UserProfileModel source);
}
