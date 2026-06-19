using FluentAssertions;
using PositiveNews.Application.Commands.Comments;

namespace PositiveNews.Application.Tests.Comments;

public class AddArticleCommentCommandValidatorTests
{
    private readonly AddArticleCommentCommandValidator _validator = new();

    [Fact]
    public void Validate_Should_Fail_When_ContentEmpty()
    {
        var result = _validator.Validate(new AddArticleCommentCommand(1, 2, "   "));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddArticleCommentCommand.Content));
    }

    [Fact]
    public void Validate_Should_Fail_When_ContentExceeds2000Characters()
    {
        var result = _validator.Validate(new AddArticleCommentCommand(1, 2, new string('x', 2001)));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddArticleCommentCommand.Content));
    }

    [Fact]
    public void Validate_Should_Pass_For_ValidCommand()
    {
        var result = _validator.Validate(new AddArticleCommentCommand(1, 2, "Great article!"));

        result.IsValid.Should().BeTrue();
    }
}
