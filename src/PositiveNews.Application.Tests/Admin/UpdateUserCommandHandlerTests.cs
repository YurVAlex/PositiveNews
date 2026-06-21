using FluentAssertions;
using NSubstitute;
using PositiveNews.Application.Abstractions.Persistence.Repositories.Write;
using PositiveNews.Application.Abstractions.Persistence.UnitOfWork;
using PositiveNews.Application.CommandHandlers.Admin;
using PositiveNews.Application.Commands.Admin;
using PositiveNews.Application.Common;
using PositiveNews.Domain.Entities;

namespace PositiveNews.Application.Tests.Admin;

public class UpdateUserCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_ReturnNotFound_When_UserMissing()
    {
        var userWriteRepository = Substitute.For<IUserWriteRepository>();
        userWriteRepository.GetByIdAsync(123, Arg.Any<CancellationToken>()).Returns((User?)null);
        var handler = new UpdateUserCommandHandler(
            userWriteRepository,
            Substitute.For<IAuditLogWriteRepository>(),
            Substitute.For<IUnitOfWork>());

        var result = await handler.Handle(new UpdateUserCommand(123, true, true, null, null, 1), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Admin.UserNotFound");
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_Should_RecordModerationAndSave_When_UserFlagsChange()
    {
        var user = User.Create("user@example.com", "Jane Doe");
        typeof(User).GetProperty(nameof(User.Id))!.SetValue(user, 10L);

        var userWriteRepository = Substitute.For<IUserWriteRepository>();
        userWriteRepository.GetByIdAsync(10, Arg.Any<CancellationToken>()).Returns(user);
        var auditLogWriteRepository = Substitute.For<IAuditLogWriteRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateUserCommandHandler(userWriteRepository, auditLogWriteRepository, unitOfWork);

        var result = await handler.Handle(new UpdateUserCommand(10, false, true, "reason", "note", 42), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        user.IsActive.Should().BeFalse();
        user.ModeratedBy.Should().Be(42);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        auditLogWriteRepository.Received(1).Add(Arg.Is<AuditLog>(log =>
            log.ChangedField == nameof(User.IsActive)
            && log.OldValue == true.ToString()
            && log.NewValue == false.ToString()));
    }

    [Fact]
    public async Task Handle_Should_ReturnValidationFailure_When_NoChangesProvided()
    {
        var user = User.Create("user@example.com", "Jane Doe");
        typeof(User).GetProperty(nameof(User.Id))!.SetValue(user, 11L);

        var userWriteRepository = Substitute.For<IUserWriteRepository>();
        userWriteRepository.GetByIdAsync(11, Arg.Any<CancellationToken>()).Returns(user);
        var handler = new UpdateUserCommandHandler(
            userWriteRepository,
            Substitute.For<IAuditLogWriteRepository>(),
            Substitute.For<IUnitOfWork>());

        var result = await handler.Handle(new UpdateUserCommand(11, true, false, null, null, 42), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Admin.UserUnchanged");
    }
}