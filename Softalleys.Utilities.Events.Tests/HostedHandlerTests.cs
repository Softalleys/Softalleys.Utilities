using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Softalleys.Utilities.Events;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Softalleys.Utilities.Events.Tests;

public class HostedHandlerTests
{
    // Test event type
    private sealed class DummyEvent : IEvent
    {
        public Guid Id { get; init; } = Guid.NewGuid();
    }

    // Hosted handler implementation that enqueues events
    private sealed class DummyHostedHandler : IEventHostedService<DummyEvent>
    {
        public readonly ConcurrentQueue<DummyEvent> Queue = new();
        private CancellationTokenSource? _cts;
        private Task? _worker;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _worker = Task.CompletedTask; // no-op worker for tests
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;

        public Task HandleAsync(DummyEvent eventData, CancellationToken cancellationToken = default)
        {
            Queue.Enqueue(eventData);
            return Task.CompletedTask;
        }
    }

    private static ServiceProvider BuildProvider()
    {
    var services = new ServiceCollection();
    services.AddLogging();

    // Register the library and scan current test assembly for handlers
    services.AddSoftalleysEvents(typeof(HostedHandlerTests).Assembly);

        return services.BuildServiceProvider();
    }

    [Fact]
    public void HostedHandler_IsSameSingleton_AsIHostedService()
    {
        using var sp = BuildProvider();

        var hostedHandler = sp.GetRequiredService<IEventHostedService<DummyEvent>>();
        var iHosted = sp.GetServices<IHostedService>().OfType<DummyHostedHandler>().SingleOrDefault();

        Assert.NotNull(iHosted);
        Assert.Same(iHosted, hostedHandler);
    }

    [Fact]
    public async Task EventBus_Invokes_HostedHandler_HandleAsync()
    {
        using var sp = BuildProvider();
        var bus = sp.GetRequiredService<IEventBus>();
        var impl = sp.GetRequiredService<DummyHostedHandler>();

        var evt = new DummyEvent();
        await bus.PublishAsync(evt);

        // Ensure the event reached the hosted handler
        Assert.Contains(evt, impl.Queue);
    }
}
