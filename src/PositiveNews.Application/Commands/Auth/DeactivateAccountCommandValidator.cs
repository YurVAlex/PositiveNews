using FluentValidation;

namespace PositiveNews.Application.Commands.Auth;

/// <summary>
/// Validates <see cref="DeactivateAccountCommand"/> input.
/// </summary>
public sealed class DeactivateAccountCommandValidator : AbstractValidator<DeactivateAccountCommand>
{
    /// <summary>
    /// Initializes validation rules for <see cref="DeactivateAccountCommand"/>.
    /// </summary>
    public DeactivateAccountCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
    }
}
