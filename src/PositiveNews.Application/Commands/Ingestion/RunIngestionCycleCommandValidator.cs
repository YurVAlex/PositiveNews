using FluentValidation;

namespace PositiveNews.Application.Commands.Ingestion;

/// <summary>
/// Placeholder validator with no additional rules beyond MediatR wiring.
/// </summary>
public sealed class RunIngestionCycleCommandValidator : AbstractValidator<RunIngestionCycleCommand>
{
    /// <summary>
    /// Initializes validation rules for <see cref="RunIngestionCycleCommand"/> (currently none).
    /// </summary>
    public RunIngestionCycleCommandValidator()
    {
    }
}
