using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Softalleys.Utilities.Events.Distributed.RabbitMQ.Options;
using Softalleys.Utilities.Events.Distributed.Receiving;

namespace Softalleys.Utilities.Events.Distributed.RabbitMQ.Receiving;

internal sealed class RabbitMqSubscriberHostedService : BackgroundService
{
    private readonly ILogger<RabbitMqSubscriberHostedService> _logger;
    private readonly IOptions<RabbitMqDistributedEventsOptions> _options;
    private readonly IDistributedEventReceiver _receiver;
    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMqSubscriberHostedService(
        ILogger<RabbitMqSubscriberHostedService> logger,
        IOptions<RabbitMqDistributedEventsOptions> options,
        IDistributedEventReceiver receiver)
    {
        _logger = logger;
        _options = options;
        _receiver = receiver;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Value.EnableSubscriber)
        {
            _logger.LogInformation("RabbitMQ subscriber disabled by configuration.");
            return Task.CompletedTask;
        }

        var o = _options.Value;
        var factory = new ConnectionFactory
        {
            HostName = o.HostName,
            Port = o.Port,
            UserName = o.UserName,
            Password = o.Password,
            VirtualHost = o.VirtualHost ?? "/",
            Ssl = { Enabled = o.UseTls }
        };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        if (o.DeclareExchange)
        {
            _channel.ExchangeDeclare(o.Exchange, o.ExchangeType, o.DurableExchange, o.AutoDeleteExchange);
        }
        if (o.DeclareQueue)
        {
            _channel.QueueDeclare(o.QueueName, o.DurableQueue, o.ExclusiveQueue, o.AutoDeleteQueue);
        }

        // Bind default routing; if per-event routing keys are provided, bind them as well
        // We will bind to "#" to receive all messages for the exchange by default
        _channel.QueueBind(o.QueueName, o.Exchange, routingKey: "#");

        _channel.BasicQos(0, o.PrefetchCount, global: false);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += OnMessageAsync;

        _channel.BasicConsume(queue: o.QueueName, autoAck: o.AutoAcknowledge, consumer: consumer);

        _logger.LogInformation("RabbitMQ subscriber started. Queue: {Queue}", o.QueueName);
        return Task.CompletedTask;
    }

    private async Task OnMessageAsync(object sender, BasicDeliverEventArgs ea)
    {
        var o = _options.Value;
        try
        {
            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (ea.BasicProperties?.Headers is not null)
            {
                foreach (var kv in ea.BasicProperties.Headers)
                {
                    headers[kv.Key] = kv.Value?.ToString() ?? string.Empty;
                }
            }

            var contentType = ea.BasicProperties?.ContentType ?? o.ContentType;
            var outcome = await _receiver.ProcessAsync(new DistributedInboundMessage(
                Payload: ea.Body.ToArray(),
                ContentType: contentType,
                Headers: headers), CancellationToken.None);

            if (!_options.Value.AutoAcknowledge)
            {
                switch (outcome)
                {
                    case InboundProcessOutcome.Success:
                        _channel!.BasicAck(ea.DeliveryTag, multiple: false);
                        break;
                    case InboundProcessOutcome.Retry:
                        _channel!.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
                        break;
                    case InboundProcessOutcome.DeadLetter:
                        _channel!.BasicReject(ea.DeliveryTag, requeue: false);
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing RabbitMQ message");
            if (!_options.Value.AutoAcknowledge)
            {
                _channel!.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
            }
        }
    }

    public override void Dispose()
    {
        try { _channel?.Close(); } catch { /* ignore */ }
        try { _connection?.Close(); } catch { /* ignore */ }
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}
