using FluentAssertions;
using NSubstitute;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Write;
using PositiveNews.Application.Abstractions.Persistence.UnitOfWork;
using PositiveNews.Application.Abstractions.Security;
using PositiveNews.Application.CommandHandlers.Auth;
using PositiveNews.Application.Commands.Auth;
using PositiveNews.Application.Common;
using PositiveNews.Domain.Constants;
using PositiveNews.Domain.Entities;

namespace PositiveNews.Application.Tests.Auth;

public class RegisterUserCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_ReturnConflict_When_EmailAlreadyRegistered()
    {
        var userReadRepository = Substitute.For<IUserReadRepository>();
        userReadRepository.EmailExistsAsync("user@example.com", Arg.Any<CancellationToken>()).Returns(true);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = CreateHandler(userReadRepository, unitOfWork: unitOfWork);

        var result = await handler.Handle(new RegisterUserCommand(" USER@example.com ", "Jane", "Password1!"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(ErrorCodes.Auth.EmailAlreadyExists);
        result.Error.Type.Should().Be(ErrorType.Conflict);
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnUnexpected_When_DefaultRoleMissing()
    {
        var userReadRepository = Substitute.For<IUserReadRepository>();
        var roleReadRepository = Substitute.For<IRoleReadRepository>();
        userReadRepository.EmailExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        roleReadRepository.FindByNameAsync(RoleNames.User, Arg.Any<CancellationToken>()).Returns((Role?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = CreateHandler(userReadRepository, roleReadRepository: roleReadRepository, unitOfWork: unitOfWork);

        var result = await handler.Handle(new RegisterUserCommand("user@example.com", "Jane", "Password1!"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(ErrorCodes.Auth.RoleMissing);
        result.Error.Type.Should().Be(ErrorType.Unexpected);
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_PersistUserIssueJwtAndSaveTwice_When_RegistrationSucceeds()
    {
        var userReadRepository = Substitute.For<IUserReadRepository>();
        var userWriteRepository = Substitute.For<IUserWriteRepository>();
        var userRoleWriteRepository = Substitute.For<IUserRoleWriteRepository>();
        var userFeedPreferencesWriteRepository = Substitute.For<IUserFeedPreferencesWriteRepository>();
        var roleReadRepository = Substitute.For<IRoleReadRepository>();
        var passwordHasher = Substitute.For<IPasswordHasherService>();
        var tokenService = Substitute.For<ITokenService>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var role = Role.Create("User");
        typeof(Role).GetProperty(nameof(Role.Id))!.SetValue(role, 5);
        var expiresAt = DateTime.UtcNow.AddHours(1);

        userReadRepository.EmailExistsAsync("user@example.com", Arg.Any<CancellationToken>()).Returns(false);
        roleReadRepository.FindByNameAsync(RoleNames.User, Arg.Any<CancellationToken>()).Returns(role);
        passwordHasher.HashPassword(Arg.Any<User>(), "Password1!").Returns("hashed-password");
        tokenService.CreateAccessToken(Arg.Any<User>(), Arg.Is<IReadOnlyCollection<string>>(r => r.Single() == "User")).Returns("access-token");
        tokenService.GetAccessTokenExpiryUtc().Returns(expiresAt);

        var handler = CreateHandler(
            userReadRepository,
            userWriteRepository,
            userRoleWriteRepository,
            userFeedPreferencesWriteRepository,
            roleReadRepository,
            passwordHasher,
            tokenService,
            null,
            unitOfWork);

        var result = await handler.Handle(new RegisterUserCommand(" USER@example.com ", " Jane ", "Password1!"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("access-token");
        result.Value.ExpiresAtUtc.Should().Be(expiresAt);
        result.Value.User.Email.Should().Be("user@example.com");
        result.Value.User.Name.Should().Be("Jane");
        result.Value.User.Roles.Should().ContainSingle().Which.Should().Be("User");

        passwordHasher.Received(1).HashPassword(Arg.Any<User>(), "Password1!");
        userWriteRepository.Received(1).Add(Arg.Is<User>(u => u.Email == "user@example.com" && u.PasswordHash == "hashed-password"));
        userRoleWriteRepository.Received(1).Add(Arg.Any<UserRole>());
        tokenService.Received(1).CreateAccessToken(Arg.Any<User>(), Arg.Any<IReadOnlyCollection<string>>());
        tokenService.Received(1).GetAccessTokenExpiryUtc();
        userFeedPreferencesWriteRepository.Received(1).AddDefault(Arg.Any<long>());
        await unitOfWork.Received(3).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static RegisterUserCommandHandler CreateHandler(
        IUserReadRepository userReadRepository,
        IUserWriteRepository? userWriteRepository = null,
        IUserRoleWriteRepository? userRoleWriteRepository = null,
        IUserFeedPreferencesWriteRepository? userFeedPreferencesWriteRepository = null,
        IRoleReadRepository? roleReadRepository = null,
        IPasswordHasherService? passwordHasher = null,
        ITokenService? tokenService = null,
        IRefreshTokenWriteRepository? refreshTokenWriteRepository = null,
        IUnitOfWork? unitOfWork = null)
        => new(
            userReadRepository,
            userWriteRepository ?? Substitute.For<IUserWriteRepository>(),
            userRoleWriteRepository ?? Substitute.For<IUserRoleWriteRepository>(),
            userFeedPreferencesWriteRepository ?? Substitute.For<IUserFeedPreferencesWriteRepository>(),
            roleReadRepository ?? Substitute.For<IRoleReadRepository>(),
            passwordHasher ?? Substitute.For<IPasswordHasherService>(),
            tokenService ?? Substitute.For<ITokenService>(),
            refreshTokenWriteRepository ?? Substitute.For<IRefreshTokenWriteRepository>(),
            unitOfWork ?? Substitute.For<IUnitOfWork>());
}
