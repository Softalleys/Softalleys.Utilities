using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Softalleys.Utilities.Events;
using Softalleys.Utilities.Events.Distributed.Configuration;
using Softalleys.Utilities.Events.Distributed;
using Softalleys.Utilities.Events.Distributed.Options;
using Softalleys.Utilities.Events.Distributed.Publishing;
using Xunit;

namespace Softalleys.Utilities.Events.Tests.Distributed;

file sealed class DummyEvent : IEvent { public string Name { get; init; } = "x"; }

file sealed class CapturePublisher : IDistributedEventPublisher
{
    private int _count;
    public int Count => _count;
    public Task PublishAsync<TEvent>(DistributedEventEnvelope<TEvent> envelope, CancellationToken cancellationToken = default) where TEvent : IEvent
    {
        Interlocked.Increment(ref _count);
        return Task.CompletedTask;
    }
}

public class DistributedEmitTests
{
    [Fact]
    public async Task Emits_No_Events_By_Default()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSoftalleysEvents();
        var capture = new CapturePublisher();
        services.AddDistributedEvents(); // no emit configuration
        services.AddSingleton<IDistributedEventPublisher>(capture);

        var sp = services.BuildServiceProvider();
        var bus = sp.GetRequiredService<IEventBus>();

        await bus.PublishAsync(new DummyEvent());
        Assert.Equal(0, capture.Count);
    }

    [Fact]
    public async Task Emits_Only_Selected_When_PublishEvent_Specified()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSoftalleysEvents();
        var capture = new CapturePublisher();
        services.AddDistributedEvents(dist =>
        {
            dist.Emit.PublishEvent<DummyEvent>();
        });
        services.AddSingleton<IDistributedEventPublisher>(capture);

        var sp = services.BuildServiceProvider();
        var bus = sp.GetRequiredService<IEventBus>();

        await bus.PublishAsync(new DummyEvent());
        Assert.Equal(1, capture.Count);
    }

    [Fact]
    public async Task Emits_All_When_AllEvents_Configured()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSoftalleysEvents();
        var capture = new CapturePublisher();
        services.AddDistributedEvents(dist =>
        {
            dist.Emit.AllEvents();
        });
        services.AddSingleton<IDistributedEventPublisher>(capture);

        var sp = services.BuildServiceProvider();
        var bus = sp.GetRequiredService<IEventBus>();

        await bus.PublishAsync(new DummyEvent());
        Assert.Equal(1, capture.Count);
    }
}
