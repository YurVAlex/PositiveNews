using MediatR;
using NSubstitute;

namespace PositiveNews.Web.Tests.TestHelpers;

/// <summary>
/// Helpers for configuring <see cref="IMediator"/> substitutes (optional; tests often configure inline).
/// </summary>
internal static class FakeMediatorFactory
{
    public static void ConfigureSendReturns<TRequest, TResponse>(
        IMediator mediator,
        TResponse response)
        where TRequest : class, IRequest<TResponse>
        where TResponse : class
    {
        mediator
            .Send(Arg.Any<TRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(response));
    }
}
