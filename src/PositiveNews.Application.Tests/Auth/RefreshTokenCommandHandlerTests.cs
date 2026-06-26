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

public class RefreshTokenCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_ReturnUnauthorized_When_RefreshTokenNotFound()
    {
        var refreshTokenReadRepository = Substitute.For<IRefreshTokenReadRepository>();
        refreshTokenReadRepository.FindValidByTokenAsync("invalid-token", Arg.Any<CancellationToken>())
            .Returns((RefreshToken?)null);
        var handler = CreateHandler(refreshTokenReadRepository);

        var result = await handler.Handle(new RefreshTokenCommand("invalid-token"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.InvalidRefreshToken");
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Handle_Should_ReturnUnauthorized_When_UserInactive()
    {
        var user = User.Create("user@example.com", "Jane");
        typeof(User).GetProperty(nameof(User.Id))!.SetValue(user, 99L);
        user.Deactivate(99);
        var refreshToken = RefreshToken.Create("valid-token", user.Id, DateTime.UtcNow.AddDays(7));
        typeof(RefreshToken).GetProperty(nameof(RefreshToken.User))!.SetValue(refreshToken, user);
        var refreshTokenReadRepository = Substitute.For<IRefreshTokenReadRepository>();
        refreshTokenReadRepository.FindValidByTokenAsync("valid-token", Arg.Any<CancellationToken>())
            .Returns(refreshToken);
        var handler = CreateHandler(refreshTokenReadRepository);

        var result = await handler.Handle(new RefreshTokenCommand("valid-token"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.UserInactive");
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Handle_Should_RevokeOldTokenAndCreateNewToken_When_RefreshTokenValid()
    {
        var user = User.Create("user@example.com", "Jane");
        user.SetPasswordHash("hash");
        UserTestHelpers.AddRole(user, Role.Create("User"));
        
        var oldRefreshToken = RefreshToken.Create("old-token", user.Id, DateTime.UtcNow.AddDays(7));
        typeof(RefreshToken).GetProperty(nameof(RefreshToken.User))!.SetValue(oldRefreshToken, user);
        var refreshTokenReadRepository = Substitute.For<IRefreshTokenReadRepository>();
        refreshTokenReadRepository.FindValidByTokenAsync("old-token", Arg.Any<CancellationToken>())
            .Returns(oldRefreshToken);

        var refreshTokenWriteRepository = Substitute.For<IRefreshTokenWriteRepository>();
        var tokenService = Substitute.For<ITokenService>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        
        tokenService.CreateRefreshTokenString().Returns("new-token");
        tokenService.GetRefreshTokenExpiryUtc().Returns(DateTime.UtcNow.AddDays(7));
        tokenService.CreateAccessToken(user, Arg.Any<IReadOnlyCollection<string>>()).Returns("new-access-token");
        tokenService.GetAccessTokenExpiryUtc().Returns(DateTime.UtcNow.AddMinutes(30));

        var handler = CreateHandler(refreshTokenReadRepository, refreshTokenWriteRepository, tokenService, unitOfWork);

        var result = await handler.Handle(new RefreshTokenCommand("old-token"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("new-access-token");
        result.Value.RefreshToken.Should().Be("new-token");
        oldRefreshToken.IsRevoked.Should().BeTrue();
        oldRefreshToken.RevokedAtUtc.Should().NotBeNull();
        refreshTokenWriteRepository.Received(1).Update(oldRefreshToken);
        refreshTokenWriteRepository.Received(1).Add(Arg.Any<RefreshToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnUserProfile_When_RefreshTokenValid()
    {
        var user = User.Create("user@example.com", "Jane");
        user.SetPasswordHash("hash");
        UserTestHelpers.AddRole(user, Role.Create("Admin"));
        
        var refreshToken = RefreshToken.Create("valid-token", user.Id, DateTime.UtcNow.AddDays(7));
        typeof(RefreshToken).GetProperty(nameof(RefreshToken.User))!.SetValue(refreshToken, user);
        var refreshTokenReadRepository = Substitute.For<IRefreshTokenReadRepository>();
        refreshTokenReadRepository.FindValidByTokenAsync("valid-token", Arg.Any<CancellationToken>())
            .Returns(refreshToken);

        var refreshTokenWriteRepository = Substitute.For<IRefreshTokenWriteRepository>();
        var tokenService = Substitute.For<ITokenService>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        
        tokenService.CreateRefreshTokenString().Returns("new-token");
        tokenService.GetRefreshTokenExpiryUtc().Returns(DateTime.UtcNow.AddDays(7));
        tokenService.CreateAccessToken(user, Arg.Any<IReadOnlyCollection<string>>()).Returns("access-token");
        tokenService.GetAccessTokenExpiryUtc().Returns(DateTime.UtcNow.AddMinutes(30));

        var handler = CreateHandler(refreshTokenReadRepository, refreshTokenWriteRepository, tokenService, unitOfWork);

        var result = await handler.Handle(new RefreshTokenCommand("valid-token"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.User.Id.Should().Be(user.Id);
        result.Value.User.Email.Should().Be(user.Email);
        result.Value.User.Name.Should().Be(user.Name);
        result.Value.User.Roles.Should().ContainSingle().Which.Should().Be("Admin");
    }

    private static RefreshTokenCommandHandler CreateHandler(
        IRefreshTokenReadRepository refreshTokenReadRepository,
        IRefreshTokenWriteRepository? refreshTokenWriteRepository = null,
        ITokenService? tokenService = null,
        IUnitOfWork? unitOfWork = null)
        => new(
            refreshTokenReadRepository,
            refreshTokenWriteRepository ?? Substitute.For<IRefreshTokenWriteRepository>(),
            tokenService ?? Substitute.For<ITokenService>(),
            unitOfWork ?? Substitute.For<IUnitOfWork>());
}
