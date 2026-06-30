using FluentAssertions;
using PositiveNews.Application.Commands.Auth;
using PositiveNews.Domain.Constants;

namespace PositiveNews.Application.Tests.Validation;

public class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _validator = new();

    public static TheoryData<string> InvalidEmails =>
    [
        "",
        "not-email",
        $"{new string('a', 292)}@example.com"
    ];

    [Theory]
    [MemberData(nameof(InvalidEmails))]
    public void Validate_Should_Fail_When_EmailInvalid(string email)
    {
        var result = _validator.Validate(new RegisterUserCommand(email, "Jane Doe", "Password1!"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterUserCommand.Email));
    }

    [Fact]
    public void Validate_Should_Fail_When_NameTooShort()
    {
        var result = _validator.Validate(new RegisterUserCommand("user@example.com", " J ", "Password1!"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("at least 2"));
    }

    [Fact]
    public void Validate_Should_Fail_When_NameTooLong()
    {
        var name = new string('n', FieldLengths.User.Name + 1);

        var result = _validator.Validate(new RegisterUserCommand("user@example.com", name, "Password1!"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains($"<= {FieldLengths.User.Name}"));
    }

    [Fact]
    public void Validate_Should_Fail_When_NameContainsUnsupportedCharacters()
    {
        var result = _validator.Validate(new RegisterUserCommand("user@example.com", "Bad<User>", "Password1!"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("unsupported characters"));
    }

    [Fact]
    public void Validate_Should_Fail_When_PasswordTooShort()
    {
        var result = _validator.Validate(new RegisterUserCommand("user@example.com", "Jane Doe", "Short1!"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterUserCommand.Password));
    }

    [Fact]
    public void Validate_Should_Fail_When_PasswordMissingUppercase()
    {
        var result = _validator.Validate(new RegisterUserCommand("user@example.com", "Jane Doe", "password1!"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("uppercase"));
    }

    [Fact]
    public void Validate_Should_Fail_When_PasswordMissingLowercase()
    {
        var result = _validator.Validate(new RegisterUserCommand("user@example.com", "Jane Doe", "PASSWORD1!"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("lowercase"));
    }

    [Fact]
    public void Validate_Should_Fail_When_PasswordMissingDigit()
    {
        var result = _validator.Validate(new RegisterUserCommand("user@example.com", "Jane Doe", "PasswordX!"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("digit"));
    }

    [Fact]
    public void Validate_Should_Fail_When_PasswordMissingSpecialCharacter()
    {
        var result = _validator.Validate(new RegisterUserCommand("user@example.com", "Jane Doe", "Password11"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("special character"));
    }

    [Fact]
    public void Validate_Should_Succeed_When_AllRulesMet()
    {
        var result = _validator.Validate(new RegisterUserCommand("user@example.com", "Jane Doe", "Password1!"));

        result.IsValid.Should().BeTrue();
    }
}
