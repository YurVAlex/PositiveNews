using FluentAssertions;
using NSubstitute;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Write;
using PositiveNews.Application.Abstractions.Persistence.UnitOfWork;
using PositiveNews.Application.Abstractions.Security;
using PositiveNews.Application.CommandHandlers.Auth;
using PositiveNews.Application.Commands.Auth;
using PositiveNews.Application.Common;
using PositiveNews.Application.Tests.TestSupport;
using PositiveNews.Domain.Entities;

namespace PositiveNews.Application.Tests.Auth;

public class LoginUserCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_ReturnUnauthorized_When_UserDoesNotExist()
    {
        var userReadRepository = Substitute.For<IUserReadRepository>();
        userReadRepository.FindByEmailWithRolesAsync("user@example.com", Arg.Any<CancellationToken>())
            .Returns((User?)null);
        var tokenService = Substitute.For<ITokenService>();
        var handler = CreateHandler(userReadRepository, tokenService: tokenService);

        var result = await handler.Handle(new LoginUserCommand(" USER@example.com ", "any"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.InvalidCredentials");
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
        tokenService.DidNotReceive().CreateAccessToken(Arg.Any<User>(), Arg.Any<IReadOnlyCollection<string>>());
        tokenService.DidNotReceive().GetAccessTokenExpiryUtc();
    }

    [Fact]
    public async Task Handle_Should_ReturnUnauthorized_When_PasswordHashMissing()
    {
        var user = User.Create("user@example.com", "Jane");
        var userReadRepository = Substitute.For<IUserReadRepository>();
        userReadRepository.FindByEmailWithRolesAsync("user@example.com", Arg.Any<CancellationToken>()).Returns(user);
        var passwordHasher = Substitute.For<IPasswordHasherService>();
        var tokenService = Substitute.For<ITokenService>();
        var handler = CreateHandler(userReadRepository, passwordHasher, tokenService);

        var result = await handler.Handle(new LoginUserCommand("user@example.com", "secret"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.InvalidCredentials");
        passwordHasher.DidNotReceive().VerifyPassword(Arg.Any<User>(), Arg.Any<string>(), Arg.Any<string>());
        tokenService.DidNotReceive().CreateAccessToken(Arg.Any<User>(), Arg.Any<IReadOnlyCollection<string>>());
    }

    [Fact]
    public async Task Handle_Should_ReturnUnauthorized_When_UserInactive()
    {
        var user = User.Create("user@example.com", "Jane");
        user.SetPasswordHash("hash");
        typeof(User).GetProperty(nameof(User.Id))!.SetValue(user, 99L);
        user.Deactivate(99);
        var userReadRepository = Substitute.For<IUserReadRepository>();
        userReadRepository.FindByEmailWithRolesAsync("user@example.com", Arg.Any<CancellationToken>()).Returns(user);
        var passwordHasher = Substitute.For<IPasswordHasherService>();
        var tokenService = Substitute.For<ITokenService>();
        var handler = CreateHandler(userReadRepository, passwordHasher, tokenService);

        var result = await handler.Handle(new LoginUserCommand("user@example.com", "Password1!"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.InvalidCredentials");
        result.Error.Message.Should().Be("The user has been deleted or blocked.");
        passwordHasher.DidNotReceive().VerifyPassword(Arg.Any<User>(), Arg.Any<string>(), Arg.Any<string>());
        tokenService.DidNotReceive().CreateAccessToken(Arg.Any<User>(), Arg.Any<IReadOnlyCollection<string>>());
    }

    [Fact]
    public async Task Handle_Should_ReturnUnauthorizedAndIncrementFailedLogin_When_PasswordInvalid()
    {
        var user = User.Create("user@example.com", "Jane");
        user.SetPasswordHash("stored-hash");
        var userReadRepository = Substitute.For<IUserReadRepository>();
        var passwordHasher = Substitute.For<IPasswordHasherService>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var tokenService = Substitute.For<ITokenService>();
        userReadRepository.FindByEmailWithRolesAsync("user@example.com", Arg.Any<CancellationToken>()).Returns(user);
        passwordHasher.VerifyPassword(user, "stored-hash", "wrong").Returns(false);
        var handler = CreateHandler(userReadRepository, passwordHasher, tokenService, null, unitOfWork);

        var result = await handler.Handle(new LoginUserCommand(" USER@example.com ", "wrong"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.InvalidCredentials");
        user.FailedLoginCount.Should().Be(1);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        tokenService.DidNotReceive().CreateAccessToken(Arg.Any<User>(), Arg.Any<IReadOnlyCollection<string>>());
    }

    [Fact]
    public async Task Handle_Should_ReturnTokenAndResetTelemetry_When_CredentialsValid()
    {
        var user = User.Create("user@example.com", "Jane");
        user.SetPasswordHash("stored-hash");
        user.RecordFailedLogin();
        UserTestHelpers.AddRole(user, Role.Create("Admin"));

        var expiresAt = DateTime.UtcNow.AddHours(1);
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
        var userReadRepository = Substitute.For<IUserReadRepository>();
        var passwordHasher = Substitute.For<IPasswordHasherService>();
        var tokenService = Substitute.For<ITokenService>();
        var refreshTokenWriteRepository = Substitute.For<IRefreshTokenWriteRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        userReadRepository.FindByEmailWithRolesAsync("user@example.com", Arg.Any<CancellationToken>()).Returns(user);
        passwordHasher.VerifyPassword(user, "stored-hash", "Password1!").Returns(true);
        tokenService.CreateAccessToken(user, Arg.Is<IReadOnlyCollection<string>>(r => r.Single() == "Admin")).Returns("access-token");
        tokenService.GetAccessTokenExpiryUtc().Returns(expiresAt);
        tokenService.CreateRefreshTokenString().Returns("refresh-token");
        tokenService.GetRefreshTokenExpiryUtc().Returns(refreshTokenExpiresAt);
        var handler = CreateHandler(userReadRepository, passwordHasher, tokenService, refreshTokenWriteRepository, unitOfWork);

        var before = DateTime.UtcNow;
        var result = await handler.Handle(new LoginUserCommand(" USER@example.com ", "Password1!"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("access-token");
        result.Value.ExpiresAtUtc.Should().Be(expiresAt);
        result.Value.RefreshToken.Should().Be("refresh-token");
        result.Value.User.Roles.Should().ContainSingle().Which.Should().Be("Admin");
        user.FailedLoginCount.Should().Be(0);
        user.LastLoginAt.Should().NotBeNull();
        user.LastLoginAt!.Value.Should().BeOnOrAfter(before);
        await unitOfWork.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
        tokenService.Received(1).CreateAccessToken(user, Arg.Any<IReadOnlyCollection<string>>());
        tokenService.Received(1).GetAccessTokenExpiryUtc();
        tokenService.Received(1).CreateRefreshTokenString();
        tokenService.Received(1).GetRefreshTokenExpiryUtc();
        refreshTokenWriteRepository.Received(1).Add(Arg.Any<RefreshToken>());
    }

    private static LoginUserCommandHandler CreateHandler(
        IUserReadRepository userReadRepository,
        IPasswordHasherService? passwordHasher = null,
        ITokenService? tokenService = null,
        IRefreshTokenWriteRepository? refreshTokenWriteRepository = null,
        IUnitOfWork? unitOfWork = null)
        => new(
            userReadRepository,
            passwordHasher ?? Substitute.For<IPasswordHasherService>(),
            tokenService ?? Substitute.For<ITokenService>(),
            refreshTokenWriteRepository ?? Substitute.For<IRefreshTokenWriteRepository>(),
            unitOfWork ?? Substitute.For<IUnitOfWork>());
}
