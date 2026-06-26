using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using PositiveNews.Application.Commands.Auth;
using PositiveNews.Application.Common;
using PositiveNews.Application.Features.Auth.Models;
using PositiveNews.Application.Queries.Auth;
using PositiveNews.Web.Api;
using PositiveNews.Web.Api.Models;
using PositiveNews.Web.Tests.TestHelpers;

namespace PositiveNews.Web.Tests.Api.Controllers;

public class AuthApiControllerTests
{
    [Fact]
    public async Task Register_Should_ReturnAuthResponse_When_CommandSucceeds()
    {
        var auth = TestDataBuilders.AuthResult();
        var mediator = Substitute.For<IMediator>();
        mediator
            .Send(Arg.Any<RegisterUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<AuthResultModel>>(auth));

        var sut = new AuthApiController(mediator) { ControllerContext = ControllerContextFactory.Create() };

        var result = await sut.Register(
            new RegisterRequest { Email = "a@b.com", Name = "Jane", Password = "Aa1!aaaa" },
            CancellationToken.None);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var body = ok.Value.Should().BeOfType<AuthResponse>().Subject;
        body.AccessToken.Should().Be(auth.AccessToken);
        body.User.Email.Should().Be(auth.User.Email);
        await mediator.Received(1).Send(
            Arg.Is<RegisterUserCommand>(c => c.Email == "a@b.com" && c.Name == "Jane"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Register_Should_ReturnProblemDetails_When_CommandFails()
    {
        var mediator = Substitute.For<IMediator>();
        var failure = Result<AuthResultModel>.Failure(
            new Error("Auth.Duplicate", "exists", ErrorType.Conflict));
        mediator
            .Send(Arg.Any<RegisterUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(failure));

        var sut = new AuthApiController(mediator) { ControllerContext = ControllerContextFactory.Create() };

        var result = await sut.Register(
            new RegisterRequest { Email = "a@b.com", Name = "Jane", Password = "Aa1!aaaa" },
            CancellationToken.None);

        var obj = result.Result.Should().BeOfType<ObjectResult>().Subject;
        obj.StatusCode.Should().Be(StatusCodes.Status409Conflict);
    }

    [Fact]
    public async Task Login_Should_ReturnAuthResponse_When_CommandSucceeds()
    {
        var auth = TestDataBuilders.AuthResult();
        var mediator = Substitute.For<IMediator>();
        mediator
            .Send(Arg.Any<LoginUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<AuthResultModel>>(auth));

        var sut = new AuthApiController(mediator) { ControllerContext = ControllerContextFactory.Create() };

        var result = await sut.Login(
            new LoginRequest { Email = "a@b.com", Password = "pw" },
            CancellationToken.None);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<AuthResponse>();
        await mediator.Received(1).Send(
            Arg.Is<LoginUserCommand>(c => c.Email == "a@b.com" && c.Password == "pw"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Login_Should_ReturnUnauthorizedProblem_When_CommandFails()
    {
        var mediator = Substitute.For<IMediator>();
        mediator
            .Send(Arg.Any<LoginUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<AuthResultModel>.Failure(
                new Error("Auth.Bad", "bad", ErrorType.Unauthorized))));

        var sut = new AuthApiController(mediator) { ControllerContext = ControllerContextFactory.Create() };

        var result = await sut.Login(new LoginRequest { Email = "a@b.com", Password = "x" }, CancellationToken.None);

        var obj = result.Result.Should().BeOfType<ObjectResult>().Subject;
        obj.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public async Task Refresh_Should_ReturnAuthResponse_When_CommandSucceeds()
    {
        var auth = TestDataBuilders.AuthResult();
        var mediator = Substitute.For<IMediator>();
        mediator
            .Send(Arg.Any<RefreshTokenCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<AuthResultModel>>(auth));

        var sut = new AuthApiController(mediator) { ControllerContext = ControllerContextFactory.Create() };

        var result = await sut.Refresh(
            new RefreshRequest { RefreshToken = "refresh-token" },
            CancellationToken.None);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<AuthResponse>();
        await mediator.Received(1).Send(
            Arg.Is<RefreshTokenCommand>(c => c.RefreshToken == "refresh-token"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Refresh_Should_ReturnUnauthorizedProblem_When_CommandFails()
    {
        var mediator = Substitute.For<IMediator>();
        mediator
            .Send(Arg.Any<RefreshTokenCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<AuthResultModel>.Failure(
                new Error("Auth.InvalidRefreshToken", "invalid", ErrorType.Unauthorized))));

        var sut = new AuthApiController(mediator) { ControllerContext = ControllerContextFactory.Create() };

        var result = await sut.Refresh(new RefreshRequest { RefreshToken = "invalid-token" }, CancellationToken.None);

        var obj = result.Result.Should().BeOfType<ObjectResult>().Subject;
        obj.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public async Task Me_Should_ReturnUnauthorizedProblem_When_UserIdClaimMissing()
    {
        var mediator = Substitute.For<IMediator>();
        var sut = new AuthApiController(mediator)
        {
            ControllerContext = ControllerContextFactory.Create(FakeUser.Anonymous())
        };

        var result = await sut.Me(CancellationToken.None);

        await mediator.DidNotReceive().Send(Arg.Any<GetCurrentUserQuery>(), Arg.Any<CancellationToken>());
        var obj = result.Result.Should().BeOfType<ObjectResult>().Subject;
        obj.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public async Task Me_Should_ReturnUnauthorizedProblem_When_UserIdNotNumeric()
    {
        var mediator = Substitute.For<IMediator>();
        var sut = new AuthApiController(mediator)
        {
            ControllerContext = ControllerContextFactory.Create(
                FakeUser.WithRoles("not-a-number", "User"))
        };

        var result = await sut.Me(CancellationToken.None);

        await mediator.DidNotReceive().Send(Arg.Any<GetCurrentUserQuery>(), Arg.Any<CancellationToken>());
        var obj = result.Result.Should().BeOfType<ObjectResult>().Subject;
        obj.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public async Task Me_Should_ReturnProfile_When_UserClaimValid()
    {
        var profile = TestDataBuilders.UserProfile();
        var mediator = Substitute.For<IMediator>();
        mediator
            .Send(Arg.Any<GetCurrentUserQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<UserProfileModel>.Success(profile)));

        var sut = new AuthApiController(mediator)
        {
            ControllerContext = ControllerContextFactory.Create(FakeUser.Standard("7"))
        };

        using var cts = new CancellationTokenSource();
        var result = await sut.Me(cts.Token);

        await mediator.Received(1).Send(
            Arg.Is<GetCurrentUserQuery>(q => q.UserId == 7),
            cts.Token);
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var body = ok.Value.Should().BeOfType<UserProfileResponse>().Subject;
        body.Email.Should().Be(profile.Email);
        body.Roles.Should().BeEquivalentTo(profile.Roles);
    }
}
