using FluentAssertions;
using PositiveNews.Application.Commands.Comments;

namespace PositiveNews.Application.Tests.Comments;

public class SubmitCommentComplaintCommandValidatorTests
{
    private readonly SubmitCommentComplaintCommandValidator _validator = new();

    [Fact]
    public void Validate_Should_Fail_When_ReasonEmpty()
    {
        var result = _validator.Validate(new SubmitCommentComplaintCommand(1, 2, 3, "   "));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SubmitCommentComplaintCommand.Reason));
    }

    [Fact]
    public void Validate_Should_Fail_When_ReasonExceeds500Characters()
    {
        var result = _validator.Validate(new SubmitCommentComplaintCommand(1, 2, 3, new string('x', 501)));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SubmitCommentComplaintCommand.Reason));
    }

    [Fact]
    public void Validate_Should_Pass_For_ValidCommand()
    {
        var result = _validator.Validate(new SubmitCommentComplaintCommand(1, 2, 3, "Spam content"));

        result.IsValid.Should().BeTrue();
    }
}
