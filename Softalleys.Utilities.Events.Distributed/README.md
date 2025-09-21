# Softalleys.Utilities.Events.Distributed

Transport-agnostic distributed event core for Softalleys.Events.

Provides:
- Envelopes + metadata
- Naming and versioning
- Serializer abstractions (System.Text.Json default)
- Type registry
- EventBus decorator to emit to transports
- Receiver to ingest external messages and dispatch through IEventBus

Usage:

builder.Services
    .AddSoftalleysEvents()
    .AddDistributedEvents(dist =>
    {
        // By default, nothing is emitted to distributed transports.
        // Choose one:
        dist.Emit.AllEvents(); // emit all
        // or select specific ones:
        // dist.Emit.PublishEvent<MyEvent>();
        dist.Naming.UseKebabCase().UseNamespacePrefix("app");
        dist.Serialization.UseSystemTextJson();
        dist.Observability.EnableTracing().EnableMetrics();
    });

Transports plug in via packages (e.g., Softalleys.Utilities.Events.Distributed.RabbitMQ) that register IDistributedEventPublisher implementations and hosted subscribers that call IDistributedEventReceiver.
