using FluentValidation;

namespace PositiveNews.Application.Commands.Ingestion;

public sealed class ProcessIngestionSourceCommandValidator : AbstractValidator<ProcessIngestionSourceCommand>
{
    public ProcessIngestionSourceCommandValidator()
    {
        RuleFor(x => x.Source).NotNull();
        RuleFor(x => x.Source.Id).GreaterThan(0);
        RuleFor(x => x.Source.Name).NotEmpty();
        RuleFor(x => x.Source.FeedUrl).NotEmpty();
        RuleFor(x => x.Source.DefaultLanguageCode).NotEmpty();
        RuleFor(x => x.TopicLookup).NotNull();
        RuleFor(x => x.IngestionSettings).NotNull();
    }
}
