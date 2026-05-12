using FluentValidation;

namespace PositiveNews.Application.Commands.Auth;

/// <summary>
/// Validates email format and length; display name length and allowed characters; password length (8–128) with uppercase, lowercase, digit, and special character requirements.
/// </summary>
public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    private const int MaxEmailLength = 300;
    private const int MinNameLength = 2;
    private const int MaxNameLength = 100;

    /// <summary>
    /// Initializes validation rules for <see cref="RegisterUserCommand"/>.
    /// </summary>
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .MaximumLength(MaxEmailLength)
            .Must(email => email.Trim().Length <= MaxEmailLength).WithMessage($"Email must be <= {MaxEmailLength} characters.")
            .Must(email => IsValidEmail(email.Trim())).WithMessage("Email must be a valid email address.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .Must(name => name.Trim().Length >= MinNameLength).WithMessage($"Name must be at least {MinNameLength} characters.")
            .Must(name => name.Trim().Length <= MaxNameLength).WithMessage($"Name must be <= {MaxNameLength} characters.")
            .Must(name => IsAllowedName(name.Trim())).WithMessage("Name contains unsupported characters.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(128)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase Latin letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase Latin letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");
    }

    /// <summary>
    /// Validates email format using data annotations.
    /// </summary>
    private static bool IsValidEmail(string email)
        => !string.IsNullOrWhiteSpace(email) && new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email);

    /// <summary>
    /// Restricts display names to letters, numbers, spaces, and a small punctuation set.
    /// </summary>
    private static bool IsAllowedName(string name)
        => System.Text.RegularExpressions.Regex.IsMatch(name, "^[\\p{L}\\p{N} .,'-]+$");
}
