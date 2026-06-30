using System.Reflection;
using FluentAssertions;
using NSubstitute;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Read;
using PositiveNews.Application.Abstractions.Persistence.UnitOfWork;
using PositiveNews.Application.CommandHandlers.Auth;
using PositiveNews.Application.Commands.Auth;
using PositiveNews.Application.Common;
using PositiveNews.Domain.Entities;

namespace PositiveNews.Application.Tests.Auth;

public class DeactivateAccountCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_ReturnNotFound_When_UserMissing()
    {
        var userReadRepository = Substitute.For<IUserReadRepository>();
        userReadRepository.FindByIdWithRolesAsync(5, Arg.Any<CancellationToken>()).Returns((User?)null);
        var handler = CreateHandler(userReadRepository);

        var result = await handler.Handle(new DeactivateAccountCommand(5), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(ErrorCodes.Auth.UserNotFound);
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_Should_ReturnConflict_When_UserAlreadyInactive()
    {
        var user = User.Create("user@example.com", "Jane");
        typeof(User).GetProperty(nameof(User.Id))!.SetValue(user, 1L);
        user.Deactivate(1);
        var userReadRepository = Substitute.For<IUserReadRepository>();
        userReadRepository.FindByIdWithRolesAsync(1, Arg.Any<CancellationToken>()).Returns(user);
        var handler = CreateHandler(userReadRepository);

        var result = await handler.Handle(new DeactivateAccountCommand(1), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(ErrorCodes.Auth.AccountInactive);
        result.Error.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task Handle_Should_DeactivateWithSelfModeration_When_UserActive()
    {
        var user = User.Create("user@example.com", "Jane");
        typeof(User).GetProperty(nameof(User.Id))!.SetValue(user, 42L);
        var userReadRepository = Substitute.For<IUserReadRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        userReadRepository.FindByIdWithRolesAsync(42, Arg.Any<CancellationToken>()).Returns(user);
        var handler = CreateHandler(userReadRepository, unitOfWork);

        var result = await handler.Handle(new DeactivateAccountCommand(42), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        user.IsActive.Should().BeFalse();
        user.ModeratedBy.Should().Be(user.Id);
        user.Email.Should().Be("deleted42@user");
        user.Name.Should().Be("Deleted user");
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static DeactivateAccountCommandHandler CreateHandler(
        IUserReadRepository userReadRepository,
        IUnitOfWork? unitOfWork = null)
        => new(userReadRepository, unitOfWork ?? Substitute.For<IUnitOfWork>());
}
