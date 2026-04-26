using FluentValidation;

namespace PositiveNews.Application.Commands.Ingestion;

public sealed class RunIngestionCycleCommandValidator : AbstractValidator<RunIngestionCycleCommand>
{
    public RunIngestionCycleCommandValidator()
    {
    }
}
