using FluentAssertions;
using PositiveNews.Application.Commands.Admin;

namespace PositiveNews.Application.Tests.Admin;

public class ModerateCommentCommandValidatorTests
{
    private readonly ModerateCommentCommandValidator _validator = new();

    [Fact]
    public void Validate_Should_Fail_When_CommentIdNotPositive()
    {
        var result = _validator.Validate(new ModerateCommentCommand(0, true, null, null, 1));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ModerateCommentCommand.CommentId));
    }

    [Fact]
    public void Validate_Should_Fail_When_ModeratorIdNotPositive()
    {
        var result = _validator.Validate(new ModerateCommentCommand(1, true, null, null, 0));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ModerateCommentCommand.ModeratorId));
    }

    [Fact]
    public void Validate_Should_Fail_When_ReasonTooLong()
    {
        var result = _validator.Validate(new ModerateCommentCommand(1, true, new string('x', 257), null, 1));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ModerateCommentCommand.Reason));
    }

    [Fact]
    public void Validate_Should_Fail_When_NoteTooLong()
    {
        var result = _validator.Validate(new ModerateCommentCommand(1, true, null, new string('x', 1025), 1));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ModerateCommentCommand.Note));
    }

    [Fact]
    public void Validate_Should_Pass_For_ValidCommand()
    {
        var result = _validator.Validate(new ModerateCommentCommand(1, false, "reason", "note", 2));

        result.IsValid.Should().BeTrue();
    }
}
