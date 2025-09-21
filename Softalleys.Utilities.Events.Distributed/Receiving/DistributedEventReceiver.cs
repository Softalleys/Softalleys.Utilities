using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Softalleys.Utilities.Events.Distributed.Naming;
using Softalleys.Utilities.Events.Distributed.Options;
using Softalleys.Utilities.Events.Distributed.Serialization;
using Softalleys.Utilities.Events.Distributed.Types;
using System.Text.Json;

namespace Softalleys.Utilities.Events.Distributed.Receiving;

internal sealed class DistributedEventReceiver : IDistributedEventReceiver
{
    private readonly IEventBus _eventBus;
    private readonly IEventSerializer _serializer;
    private readonly IEventNameResolver _namer;
    private readonly IEventTypeRegistry _registry;
    private readonly ILogger<DistributedEventReceiver>? _logger;

    public DistributedEventReceiver(IEventBus eventBus, IEventSerializer serializer, IEventNameResolver namer, IEventTypeRegistry registry, ILogger<DistributedEventReceiver>? logger = null)
    {
        _eventBus = eventBus;
        _serializer = serializer;
        _namer = namer;
        _registry = registry;
        _logger = logger;
    }

    public async Task<InboundProcessOutcome> ProcessAsync(DistributedInboundMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogDebug("Inbound message received: ContentType={ContentType}, Name={Name}, Version={Version}, Source={Source}, HeaderCount={HeaderCount}",
                message.ContentType, message.EventName, message.Version, message.Source, message.Headers?.Count ?? 0);

            // First try: assume our standard envelope JSON
            DistributedEventEnvelopeRaw envelope;
            try
            {
                envelope = _serializer.Deserialize(message.Payload.Span, message.ContentType);
            }
            catch
            {
                envelope = default!;
            }

            if (envelope is not null && envelope.Meta is not null)
            {
                _logger?.LogDebug("Envelope detected with meta: Name={Name}, Version={Version}", envelope.Meta.Name, envelope.Meta.Version);
                var type = ResolveType(envelope.Meta.Name, envelope.Meta.Version);
                if (type is null)
                {
                    _logger?.LogWarning("Unknown event type for name {Name} v{Version}. Dead-lettering.", envelope.Meta.Name, envelope.Meta.Version);
                    return InboundProcessOutcome.DeadLetter;
                }

                _logger?.LogDebug("Resolved CLR type {Type} for event {Name} v{Version}", type.FullName, envelope.Meta.Name, envelope.Meta.Version);
                var obj = Rehydrate(type, envelope.Data);
                await PublishDynamic(obj, cancellationToken).ConfigureAwait(false);
                return InboundProcessOutcome.Success;
            }

            // Fallback: raw payload with provided eventName/version
            if (string.IsNullOrWhiteSpace(message.EventName) || message.Version is null)
            {
                _logger?.LogWarning("Inbound message missing event name or version. Dead-lettering.");
                return InboundProcessOutcome.DeadLetter;
            }

            var clrType = ResolveType(message.EventName!, message.Version!.Value);
            if (clrType is null)
            {
                _logger?.LogWarning("Unknown event type for name {Name} v{Version}. Dead-lettering.", message.EventName, message.Version);
                return InboundProcessOutcome.DeadLetter;
            }

            _logger?.LogDebug("Resolved CLR type {Type} for event {Name} v{Version} (raw mode)", clrType.FullName, message.EventName, message.Version);
            object deserialized;
            try
            {
                deserialized = JsonSerializer.Deserialize(message.Payload.Span, clrType, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
                               ?? throw new InvalidOperationException("Payload deserialized to null");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to deserialize payload for {Name} v{Version}", message.EventName, message.Version);
                return InboundProcessOutcome.DeadLetter;
            }

            await PublishDynamic(deserialized, cancellationToken).ConfigureAwait(false);
            _logger?.LogDebug("Published inbound event to local bus: {Name} v{Version}", message.EventName ?? "(enveloped)", message.Version ?? -1);
            return InboundProcessOutcome.Success;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Unexpected error while processing inbound message");
            return InboundProcessOutcome.Retry;
        }
    }

    private Type? ResolveType(string name, int version)
        => _registry.TryGetType(name, version, out var t) ? t : null;

    private static object Rehydrate(Type type, object jsonElementOrObj)
    {
        if (jsonElementOrObj is JsonElement je)
        {
            return je.Deserialize(type, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
                   ?? throw new InvalidOperationException("Envelope contained null data");
        }
        if (type.IsInstanceOfType(jsonElementOrObj)) return jsonElementOrObj;
        var bytes = JsonSerializer.SerializeToUtf8Bytes(jsonElementOrObj, jsonElementOrObj.GetType());
        return JsonSerializer.Deserialize(bytes, type) ?? throw new InvalidOperationException("Rehydrate failed");
    }

    private Task PublishDynamic(object evt, CancellationToken ct)
    {
        var method = typeof(IEventBus).GetMethod(nameof(IEventBus.PublishAsync))!;
        var generic = method.MakeGenericMethod(evt.GetType());
        var task = (Task)generic.Invoke(_eventBus, new object[] { evt, ct })!;
        return task;
    }
}
