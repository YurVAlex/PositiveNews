using FluentValidation;

namespace PositiveNews.Application.Commands.Auth;

/// <summary>
/// Validates the RefreshTokenCommand payload.
/// </summary>
public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    /// <summary>
    /// Refresh token is a Base64-encoded 64-byte random string = 88 characters max.
    /// </summary>
    private const int MaxRefreshTokenLength = 88;

    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token cannot be empty.")
            .MaximumLength(MaxRefreshTokenLength)
            .WithMessage($"Refresh token cannot exceed {MaxRefreshTokenLength} characters.");
    }
}