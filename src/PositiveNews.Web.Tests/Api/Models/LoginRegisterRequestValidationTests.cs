using FluentAssertions;
using PositiveNews.Application.Commands.Auth;

namespace PositiveNews.Web.Tests.Api.Models;

/// <summary>
/// Validates the same rules applied to API-bound credentials via <see cref="RegisterUserCommand"/> / <see cref="LoginUserCommand"/> (MediatR pipeline).
/// </summary>
public class LoginRegisterRequestValidationTests
{
    private readonly RegisterUserCommandValidator _registerValidator = new();
    private readonly LoginUserCommandValidator _loginValidator = new();

    [Fact]
    public void Register_Should_Pass_When_ValidCommand()
    {
        var cmd = new RegisterUserCommand("user@test.com", "Jane", "Aa1!aaaa");

        var result = _registerValidator.Validate(cmd);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Register_Should_Fail_When_EmailInvalid(string email)
    {
        var cmd = new RegisterUserCommand(email, "Jane", "Aa1!aaaa");

        var result = _registerValidator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterUserCommand.Email));
    }

    [Fact]
    public void Register_Should_Fail_When_PasswordWeak()
    {
        var cmd = new RegisterUserCommand("user@test.com", "Jane", "short");

        var result = _registerValidator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterUserCommand.Password));
    }

    [Fact]
    public void Register_Should_Fail_When_NameTooShort()
    {
        var cmd = new RegisterUserCommand("user@test.com", "J", "Aa1!aaaa");

        var result = _registerValidator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterUserCommand.Name));
    }

    [Fact]
    public void Login_Should_Pass_When_ValidCommand()
    {
        var cmd = new LoginUserCommand("user@test.com", "secret123");

        var result = _loginValidator.Validate(cmd);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Login_Should_Fail_When_EmailInvalid(string email)
    {
        var cmd = new LoginUserCommand(email, "pw");

        var result = _loginValidator.Validate(cmd);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Login_Should_Fail_When_PasswordEmpty()
    {
        var cmd = new LoginUserCommand("user@test.com", "");

        var result = _loginValidator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginUserCommand.Password));
    }
}
