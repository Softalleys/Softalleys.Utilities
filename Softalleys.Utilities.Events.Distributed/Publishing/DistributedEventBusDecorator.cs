using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Softalleys.Utilities.Events.Distributed.Naming;
using Softalleys.Utilities.Events.Distributed.Options;
using Softalleys.Utilities.Events.Distributed.Serialization;

namespace Softalleys.Utilities.Events.Distributed.Publishing;

internal sealed class DistributedEventBusDecorator : IEventBus
{
    private readonly IEventBus _inner;
    private readonly IEnumerable<IDistributedEventPublisher> _publishers;
    private readonly DistributedEventsOptions _options;
    private readonly IEventNameResolver _namer;
    private readonly IEventSerializer _serializer;
    private readonly ILogger<DistributedEventBusDecorator>? _logger;

    public DistributedEventBusDecorator(
        IEventBus inner,
        IEnumerable<IDistributedEventPublisher> publishers,
        IOptions<DistributedEventsOptions> options,
        IEventNameResolver namer,
        IEventSerializer serializer,
        ILogger<DistributedEventBusDecorator>? logger = null)
    {
        _inner = inner;
        _publishers = publishers;
        _options = options.Value;
        _namer = namer;
        _serializer = serializer;
        _logger = logger;
    }

    public async Task PublishAsync<TEvent>(TEvent eventData, CancellationToken ct = default) where TEvent : IEvent
    {
        var localTask = _inner.PublishAsync(eventData, ct);

        if (!_options.EmitFilter.ShouldEmit(typeof(TEvent)) || !_publishers.Any())
        {
            _logger?.LogDebug("Distributed emit skipped for {EventType}. Filtered={Filtered} Publishers={Publishers}",
                typeof(TEvent).FullName, !_options.EmitFilter.ShouldEmit(typeof(TEvent)), _publishers.Count());
            await localTask.ConfigureAwait(false);
            return;
        }

        var name = _namer.GetName(typeof(TEvent));
        var version = _namer.GetVersion(typeof(TEvent));
        _logger?.LogDebug("Distributed emit for {EventType} as {Name} v{Version} via {PublisherCount} publishers; mode={Mode}",
            typeof(TEvent).FullName, name, version, _publishers.Count(), _options.DeliveryMode);

        var envelope = new DistributedEventEnvelope<TEvent>(
            eventData,
            new DistributedEventMetadata
            {
                Name = name,
                Type = typeof(TEvent).FullName ?? typeof(TEvent).Name,
                Version = version
            });

        Task emitTask = Task.WhenAll(_publishers.Select(p => p.PublishAsync(envelope, ct)));

        switch (_options.DeliveryMode)
        {
            case DeliveryMode.LocalFirstEmitAsync:
                await localTask.ConfigureAwait(false);
                _ = emitTask.ContinueWith(t =>
                {
                    if (t.Exception != null)
                        _logger?.LogError(t.Exception, "Distributed emit failed for {EventName}", envelope.Meta.Name);
                    else
                        _logger?.LogDebug("Distributed emit completed for {EventName} v{Version}", envelope.Meta.Name, envelope.Meta.Version);
                }, TaskScheduler.Default);
                break;
            case DeliveryMode.LocalAndEmitInParallel:
                await Task.WhenAll(localTask, emitTask).ConfigureAwait(false);
                _logger?.LogDebug("Local and distributed publish completed for {EventName} v{Version}", envelope.Meta.Name, envelope.Meta.Version);
                break;
            case DeliveryMode.RequireDistributedSuccess:
                await localTask.ConfigureAwait(false);
                await emitTask.ConfigureAwait(false);
                _logger?.LogDebug("Distributed publish required and completed for {EventName} v{Version}", envelope.Meta.Name, envelope.Meta.Version);
                break;
        }
    }
}

internal sealed class NoOpPublisher : IDistributedEventPublisher
{
    public Task PublishAsync<TEvent>(DistributedEventEnvelope<TEvent> envelope, CancellationToken cancellationToken = default) where TEvent : IEvent
        => Task.CompletedTask;
}
