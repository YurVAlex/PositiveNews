using FluentValidation;
using PositiveNews.Domain.Constants;

namespace PositiveNews.Application.Commands.Auth;

/// <summary>
/// Ensures email is non-empty, within length limits, and formatted as an email address; password is required and capped at 128 characters.
/// </summary>
public sealed class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    /// <summary>
    /// Initializes validation rules for <see cref="LoginUserCommand"/>.
    /// </summary>
    public LoginUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .MaximumLength(FieldLengths.User.Email)
            .Must(email => email.Trim().Length <= FieldLengths.User.Email).WithMessage($"Email must be <= {FieldLengths.User.Email} characters.")
            .Must(email => IsValidEmail(email.Trim())).WithMessage("Email must be a valid email address.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .MaximumLength(FieldLengths.Auth.PasswordMax);
    }

    /// <summary>
    /// Delegates to <see cref="System.ComponentModel.DataAnnotations.EmailAddressAttribute"/> after a whitespace guard.
    /// </summary>
    private static bool IsValidEmail(string email)
        => !string.IsNullOrWhiteSpace(email) && new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email);
}
