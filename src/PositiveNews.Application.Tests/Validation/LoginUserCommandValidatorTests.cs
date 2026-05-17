using FluentAssertions;
using PositiveNews.Application.Commands.Auth;

namespace PositiveNews.Application.Tests.Validation;

public class LoginUserCommandValidatorTests
{
    private readonly LoginUserCommandValidator _validator = new();

    [Fact]
    public void Validate_Should_Fail_When_EmailEmpty()
    {
        var result = _validator.Validate(new LoginUserCommand("", "secret"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginUserCommand.Email));
    }

    [Fact]
    public void Validate_Should_Fail_When_PasswordEmpty()
    {
        var result = _validator.Validate(new LoginUserCommand("user@example.com", ""));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginUserCommand.Password));
    }

    [Fact]
    public void Validate_Should_Fail_When_EmailInvalid()
    {
        var result = _validator.Validate(new LoginUserCommand("not-an-email", "secret"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Email must be a valid email address.");
    }

    [Fact]
    public void Validate_Should_Fail_When_TrimmedEmailExceedsMaxLength()
    {
        var localPart = new string('a', 292);
        var email = $"{localPart}@example.com";

        var result = _validator.Validate(new LoginUserCommand(email, "secret"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Email must be <= 300 characters.");
    }

    [Fact]
    public void Validate_Should_Fail_When_PasswordExceeds128Characters()
    {
        var password = new string('p', 129);

        var result = _validator.Validate(new LoginUserCommand("user@example.com", password));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginUserCommand.Password));
    }

    [Fact]
    public void Validate_Should_Succeed_When_EmailAndPasswordWithinRules()
    {
        var result = _validator.Validate(new LoginUserCommand("user.name+tag@example.com", "Password1!"));

        result.IsValid.Should().BeTrue();
    }
}
