using System.Buffers;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Softalleys.Utilities.Events.Distributed;
using Softalleys.Utilities.Events.Distributed.Serialization;
using Softalleys.Utilities.Events.Distributed.Naming;
using Softalleys.Utilities.Events.Distributed.Publishing;
using Softalleys.Utilities.Events.Distributed.RabbitMQ.Options;
using Softalleys.Utilities.Events.Distributed.RabbitMQ.Routing;

namespace Softalleys.Utilities.Events.Distributed.RabbitMQ.Publishing;

internal sealed class RabbitMqDistributedEventPublisher : IDistributedEventPublisher, IDisposable
{
    private readonly ILogger<RabbitMqDistributedEventPublisher> _logger;
    private readonly IEventSerializer _serializer;
    private readonly IEventNameResolver _nameResolver;
    private readonly IOptions<RabbitMqDistributedEventsOptions> _options;
    private readonly IRabbitMqRoutingResolver _routingResolver;
    private readonly ConnectionFactory _factory;
    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMqDistributedEventPublisher(
        ILogger<RabbitMqDistributedEventPublisher> logger,
        IEventSerializer serializer,
        IEventNameResolver nameResolver,
        IOptions<RabbitMqDistributedEventsOptions> options,
        IRabbitMqRoutingResolver routingResolver)
    {
        _logger = logger;
        _serializer = serializer;
        _nameResolver = nameResolver;
    _options = options;
    _routingResolver = routingResolver;

        var o = options.Value;
        _factory = new ConnectionFactory
        {
            HostName = o.HostName,
            Port = o.Port,
            UserName = o.UserName,
            Password = o.Password,
            VirtualHost = o.VirtualHost ?? "/",
            Ssl = { Enabled = o.UseTls }
        };
    }

    public Task PublishAsync<TEvent>(DistributedEventEnvelope<TEvent> envelope, CancellationToken ct) where TEvent : IEvent
    {
        EnsureChannel();
        var ch = _channel!;
    var o = _options.Value;

    // Prepare properties
        var props = ch.CreateBasicProperties();
        props.ContentType = o.ContentType;
        props.DeliveryMode = 2; // persistent
        props.MessageId = envelope.Meta.EventId.ToString();
        props.CorrelationId = envelope.Meta.CorrelationId ?? envelope.Meta.EventId.ToString();
        props.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        props.Headers = envelope.Meta.Headers?.ToDictionary(k => k.Key, v => (object?)v.Value) ?? new Dictionary<string, object?>();
        props.Headers["eventName"] = envelope.Meta.Name;
        props.Headers["eventVersion"] = envelope.Meta.Version;

        // Routing
        var (exchange, routingKey, mandatory) = _routingResolver.Resolve(envelope.Meta);

        if (o.DeclareExchange)
        {
            ch.ExchangeDeclare(exchange, o.ExchangeType, o.DurableExchange, o.AutoDeleteExchange);
        }

        // Serialize
        var payload = _serializer.Serialize(envelope);

        ch.BasicPublish(
            exchange: exchange,
            routingKey: routingKey,
            mandatory: mandatory,
            basicProperties: props,
            body: payload);

        _logger.LogDebug("Published event {Event} v{Version} to exchange {Exchange} with routingKey {RoutingKey}", envelope.Meta.Name, envelope.Meta.Version, exchange, routingKey);

        return Task.CompletedTask;
    }

    private void EnsureChannel()
    {
        if (_channel is { IsClosed: false }) return;
        _connection?.Dispose();
        _channel?.Dispose();
        _connection = _factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    public void Dispose()
    {
        try { _channel?.Close(); } catch { /* ignore */ }
        try { _connection?.Close(); } catch { /* ignore */ }
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
