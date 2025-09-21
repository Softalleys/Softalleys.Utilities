# Softalleys.Utilities.Events.Distributed.RabbitMQ

RabbitMQ transport for `Softalleys.Utilities.Events.Distributed`.

Features:
- Publisher implementing `IDistributedEventPublisher`
- Hosted subscriber that receives messages and forwards them to `IDistributedEventReceiver`
- Options configurable via code, appsettings.json, or environment variables
- Per-event overrides (routingKey, exchange, mandatory flag, headers)

Quick start (Program.cs):

```csharp
builder.Services
    .AddSoftalleysEvents()
    .AddDistributedEvents(dist =>
    {
        dist.Emit.AllEvents(); // or PublishEvent<YourEvent>()
        dist.Serialization.UseSystemTextJson();
        dist.Naming.UseKebabCase();
        dist.UseRabbitMq(rb => rb.Configure(o =>
        {
            o.HostName = "localhost";
            o.Exchange = "softalleys.events";
            o.QueueName = "softalleys.events.dev";
            o.Events["your-event-name"] = new()
            {
                RoutingKey = "your.event",
            };
        }));
    });
```

appsettings.json:

```json
{
  "Softalleys": {
    "Events": {
      "Distributed": {
        "RabbitMQ": {
          "HostName": "localhost",
          "Exchange": "softalleys.events",
          "QueueName": "softalleys.events.local",
          "RoutingKeyTemplate": "{name}.{version}",
          "Events": {
            "my-event": { "RoutingKey": "my.event" }
          }
        }
      }
    }
  }
}
```

Environment variables (ASP.NET Core naming):

```
Softalleys__Events__Distributed__RabbitMQ__HostName=localhost
Softalleys__Events__Distributed__RabbitMQ__Exchange=softalleys.events
Softalleys__Events__Distributed__RabbitMQ__QueueName=softalleys.events.local
```
