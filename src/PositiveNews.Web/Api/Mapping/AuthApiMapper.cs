using PositiveNews.Application.Features.Auth.Models;
using PositiveNews.Web.Api.Models;
using Riok.Mapperly.Abstractions;

namespace PositiveNews.Web.Api.Mapping;

[Mapper]
public static partial class AuthApiMapper
{
    public static partial AuthResponse ToAuthResponse(this AuthResultModel source);
    public static partial UserProfileResponse ToUserProfileResponse(this UserProfileModel source);
}
