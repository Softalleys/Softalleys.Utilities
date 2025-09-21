using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;

namespace Softalleys.Utilities.Events.Distributed.RabbitMQ.Options;

public sealed class RabbitMqDistributedEventsOptions
{
    // Connection
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string? VirtualHost { get; set; }
    public bool UseTls { get; set; }

    // Exchange and topology
    public string Exchange { get; set; } = "softalleys.events";
    public string ExchangeType { get; set; } = "topic"; // default topic
    public bool DurableExchange { get; set; } = true;
    public bool AutoDeleteExchange { get; set; } = false;
    public bool DeclareExchange { get; set; } = true;

    // Publish defaults
    public bool Mandatory { get; set; } = false;
    public string RoutingKeyTemplate { get; set; } = "{name}.{version}"; // can be overridden per-event

    // Queue (receive) defaults
    public string QueueName { get; set; } = "softalleys.events"; // can be suffixed by env
    public bool DeclareQueue { get; set; } = true;
    public bool DurableQueue { get; set; } = true;
    public bool ExclusiveQueue { get; set; } = false;
    public bool AutoDeleteQueue { get; set; } = false;
    public ushort PrefetchCount { get; set; } = 50;
    public bool AutoAcknowledge { get; set; } = false; // we will ack manually after success

    // Content type
    public string ContentType { get; set; } = "application/json";

    // Enable hosted consumer
    public bool EnableSubscriber { get; set; } = true;

    // Per-event overrides by resolved event name (after Distributed naming resolver)
    public Dictionary<string, RabbitMqEventConfig> Events { get; set; } = new();

    public RabbitMqEventConfig GetEventConfigOrDefault(string eventName)
    {
        return Events.TryGetValue(eventName, out var cfg) ? cfg : new RabbitMqEventConfig();
    }
}

public sealed class RabbitMqEventConfig
{
    public string? RoutingKey { get; set; }
    public string? Exchange { get; set; }
    public bool? Mandatory { get; set; }
    public Dictionary<string, object?> Headers { get; set; } = new();
}
