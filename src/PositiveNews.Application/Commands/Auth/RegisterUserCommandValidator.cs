using FluentValidation;

namespace PositiveNews.Application.Commands.Auth;

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    private const int MaxEmailLength = 300;
    private const int MinNameLength = 2;
    private const int MaxNameLength = 100;

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

    private static bool IsValidEmail(string email)
        => !string.IsNullOrWhiteSpace(email) && new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email);

    private static bool IsAllowedName(string name)
        => System.Text.RegularExpressions.Regex.IsMatch(name, "^[\\p{L}\\p{N} .,'-]+$");
}
