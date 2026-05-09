using MediatR;
using PositiveNews.Application.Common;
using PositiveNews.Application.Features.Auth.Models;

namespace PositiveNews.Application.Commands.Auth;

public sealed record LoginUserCommand(string Email, string Password) : IRequest<Result<AuthResultModel>>;
