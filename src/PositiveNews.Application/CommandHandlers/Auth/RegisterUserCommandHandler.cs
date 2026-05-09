using MediatR;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Write;
using PositiveNews.Application.Abstractions.Persistence.UnitOfWork;
using PositiveNews.Application.Abstractions.Security;
using PositiveNews.Application.Commands.Auth;
using PositiveNews.Application.Common;
using PositiveNews.Application.Features.Auth.Models;
using PositiveNews.Domain.Entities;

namespace PositiveNews.Application.CommandHandlers.Auth;

public sealed class RegisterUserCommandHandler(
    IUserReadRepository userReadRepository,
    IUserWriteRepository userWriteRepository,
    IUserRoleWriteRepository userRoleWriteRepository,
    IRoleReadRepository roleReadRepository,
    IPasswordHasherService passwordHasherService,
    ITokenService tokenService,
    IUnitOfWork unitOfWork) : IRequestHandler<RegisterUserCommand, Result<AuthResultModel>>
{
    public async Task<Result<AuthResultModel>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var normalizedName = request.Name.Trim();

        if (await userReadRepository.EmailExistsAsync(normalizedEmail, cancellationToken))
        {
            return Result<AuthResultModel>.Failure(
                new Error("Auth.EmailAlreadyExists", "A user with this email already exists.", ErrorType.Conflict));
        }

        var userRole = await roleReadRepository.FindByNameAsync("User", cancellationToken);
        if (userRole is null)
        {
            return Result<AuthResultModel>.Failure(
                new Error("Auth.RoleMissing", "Default 'User' role is missing.", ErrorType.Unexpected));
        }

        var user = User.Create(normalizedEmail, normalizedName);
        user.ConfirmEmail();
        user.SetPasswordHash(passwordHasherService.HashPassword(user, request.Password));
        user.RecordSuccessfulLogin();

        userWriteRepository.Add(user);
        userRoleWriteRepository.Add(UserRole.Create(userRole.Id, user));
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var roles = new[] { userRole.Name };
        return Result<AuthResultModel>.Success(new AuthResultModel
        {
            AccessToken = tokenService.CreateAccessToken(user, roles),
            ExpiresAtUtc = tokenService.GetAccessTokenExpiryUtc(),
            User = new UserProfileModel
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                Roles = roles
            }
        });
    }
}
