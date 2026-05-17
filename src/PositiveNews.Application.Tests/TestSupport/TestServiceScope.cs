using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace PositiveNews.Application.Tests.TestSupport;

internal sealed class TestServiceProvider(object? mediatorInstance) : IServiceProvider
{
    public object? GetService(Type serviceType)
        => serviceType == typeof(IMediator) ? mediatorInstance : null;
}

internal sealed class TestServiceScope : IServiceScope
{
    public TestServiceScope(IServiceProvider serviceProvider) => ServiceProvider = serviceProvider;

    public IServiceProvider ServiceProvider { get; }

    public void Dispose()
    {
    }
}

internal sealed class TestServiceScopeFactory : IServiceScopeFactory
{
    private readonly Queue<IServiceScope> _queue;

    public TestServiceScopeFactory(params IServiceScope[] scopes)
        => _queue = new Queue<IServiceScope>(scopes);

    public IServiceScope CreateScope()
        => _queue.Dequeue();
}
