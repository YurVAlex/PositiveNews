using MediatR;
using PositiveNews.Application.Common;
using PositiveNews.Application.Features.Auth.Models;

namespace PositiveNews.Application.Queries.Auth;

public sealed record GetCurrentUserQuery(long UserId) : IRequest<Result<UserProfileModel>>;
