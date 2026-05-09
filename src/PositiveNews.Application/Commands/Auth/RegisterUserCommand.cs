using MediatR;
using PositiveNews.Application.Common;
using PositiveNews.Application.Features.Auth.Models;

namespace PositiveNews.Application.Commands.Auth;

public sealed record RegisterUserCommand(string Email, string Name, string Password) : IRequest<Result<AuthResultModel>>;
