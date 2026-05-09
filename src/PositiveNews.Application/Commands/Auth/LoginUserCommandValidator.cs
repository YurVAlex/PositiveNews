using FluentValidation;

namespace PositiveNews.Application.Commands.Auth;

public sealed class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    private const int MaxEmailLength = 300;

    public LoginUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .MaximumLength(MaxEmailLength)
            .Must(email => email.Trim().Length <= MaxEmailLength).WithMessage($"Email must be <= {MaxEmailLength} characters.")
            .Must(email => IsValidEmail(email.Trim())).WithMessage("Email must be a valid email address.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .MaximumLength(128);
    }

    private static bool IsValidEmail(string email)
        => !string.IsNullOrWhiteSpace(email) && new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email);
}
