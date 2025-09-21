# Softalleys.Utilities.Events.Distributed.GooglePubSub

Google Cloud Pub/Sub transport for `Softalleys.Utilities.Events.Distributed` with both delivery modes supported:

- Push: Pub/Sub sends HTTP POST requests to your service (Minimal API endpoint provided)
- Pull: Your service runs a background subscriber that pulls and processes messages

This package aims to give you an out‑of‑the‑box event-driven integration with minimal application code: configure once and handle your events.

## What is Google Cloud Pub/Sub (quick intro)

Google Cloud Pub/Sub is a fully-managed messaging service that implements pub/sub semantics:

- Producers publish messages to a topic
- One or many subscriptions are attached to a topic
- Each subscription delivers messages to a consumer (push via HTTP or pull via client)
- Delivery is at-least-once; your handlers should be idempotent

Fan-out is modeled by creating multiple subscriptions on the same topic (one push endpoint per subscription).

## Why Softalleys.Utilities.Events + Pub/Sub

`Softalleys.Utilities.Events` gives you a clean event-driven developer experience:

- Strongly-typed events and handlers
- Clear naming and versioning
- Pluggable distributed transports

With this package you can emit and receive those events over Google Pub/Sub with almost no glue code.

---

## Features

- Publish events to a configured Pub/Sub topic
- Receive events via:
  - Push (HTTP POST to a Minimal API endpoint you map)
  - Pull (BackgroundService that streams messages)
- Configurable naming and serialization (inherits from the base distributed events builder)
- JWT/OIDC validation for push requests (enable in production; disable for emulator)
- Dev-friendly options for emulator and local setups

---

## Installation

Add the package reference to your project:

```xml
<ItemGroup>
  <PackageReference Include="Softalleys.Utilities.Events.Distributed.GooglePubSub" Version="x.y.z" />
  <!-- Also reference Softalleys.Utilities.Events if not already present -->
  <PackageReference Include="Softalleys.Utilities.Events" Version="x.y.z" />
  <PackageReference Include="Softalleys.Utilities.Events.Distributed" Version="x.y.z" />
  <!-- Serializer of your choice (System.Text.Json shown via the builder API) -->
</ItemGroup>
```

---

## Quick start: minimal Sender (Ping)

Program.cs:

```csharp
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Softalleys.Utilities.Events;
using Softalleys.Utilities.Events.Distributed.Configuration;
using Softalleys.Utilities.Events.Distributed.GooglePubSub;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddSoftalleysEvents(typeof(PingRequested).Assembly)
    .AddDistributedEvents(dist =>
    {
        dist.Emit.AllEvents();
        dist.Emit.RequireDistributedSuccess(); // wait for publish completion (useful for console apps)
        dist.Serialization.UseSystemTextJson();
        dist.Naming.UseKebabCase().Map<PingRequested>("ping-requested", 1);

        dist.UseGooglePubSub(g => g.Configure(o =>
        {
            o.ProjectId = "local-project";   // or your GCP project
            o.TopicId   = "events";

            // Local emulator convenience: topic will be auto-created on first publish by the publisher
            // In production, provision topic/subscriptions with IaC (Terraform, gcloud, etc.)
        }));
    });

var app = builder.Build();

await using var scope = app.Services.CreateAsyncScope();
var bus = scope.ServiceProvider.GetRequiredService<IEventBus>();
await bus.PublishAsync(new PingRequested { Message = "hello from ping" });
```

Notes:
- No Pub/Sub admin code is needed in your app; the transport handles publishing.
- For the emulator, set `PUBSUB_EMULATOR_HOST` to point to the emulator host.

---

## Quick start: minimal Receiver (Pong) via Push

Program.cs (Minimal API):

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Softalleys.Utilities.Events;
using Softalleys.Utilities.Events.Distributed.Configuration;
using Softalleys.Utilities.Events.Distributed.GooglePubSub;
using Softalleys.Utilities.Events.Distributed.GooglePubSub.Receiving;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddSoftalleysEvents(typeof(PingRequested).Assembly)
    .AddDistributedEvents(dist =>
    {
        dist.Serialization.UseSystemTextJson();
        dist.Naming.UseKebabCase().Map<PingRequested>("ping-requested", 1);

        dist.UseGooglePubSub(g => g.Configure(o =>
        {
            o.ProjectId = "local-project";
            o.TopicId   = "events";

            // Push receiver security:
            // In production: keep RequireJwtValidation = true and set Audience/Issuer
            // For local emulator: set RequireJwtValidation = false (no token is sent)
            o.RequireJwtValidation = false; // emulator/dev only

            // Optional: customize the endpoint path
            // o.SubscribePath = "/google-pubsub/receive";
        }));
    });

builder.Services.AddScoped<IEventHandler<PingRequested>, PingRequestedHandler>();

var app = builder.Build();

// Expose the push endpoint. Pub/Sub will POST here.
app.MapGooglePubSubReceiver();

// Health
app.MapGet("/health", () => Results.Ok("ok"));

await app.RunAsync();

public sealed class PingRequestedHandler : IEventHandler<PingRequested>
{
    public Task HandleAsync(PingRequested e, CancellationToken ct = default)
    {
        Console.WriteLine($"[PONG] Received: {e.Message}");
        return Task.CompletedTask;
    }
}
```

To receive messages, create a push subscription on your topic that targets the service URL + `SubscribePath`.

Important:
- A push subscription has a single `PushEndpoint`. To fan out to multiple services, create multiple subscriptions.
- In production, use HTTPS and OIDC tokens. Validate:
  - Issuer: `https://accounts.google.com`
  - Audience: your Cloud Run service URL (or configured audience)

---

## Quick start: minimal Receiver (Pong) via Pull

Program.cs (Generic Host or Web):

```csharp
builder.Services
    .AddSoftalleysEvents(typeof(PingRequested).Assembly)
    .AddDistributedEvents(dist =>
    {
        dist.Serialization.UseSystemTextJson();
        dist.Naming.UseKebabCase().Map<PingRequested>("ping-requested", 1);
        dist.UseGooglePubSub(g => g.Configure(o =>
        {
            o.ProjectId = "local-project";
            o.TopicId = "events";

            // Pull mode
            o.EnablePullSubscriber = true;
            o.SubscriptionId = "pong-sub"; // existing subscription (create via IaC)

            // Dev convenience (emulator only):
            // o.AutoProvisionSubscription = true; // creates the pull subscription if missing
        }));
    });
```

The package hosts a background pull subscriber that streams messages and dispatches to your `IEventHandler<TEvent>`.

---

## Configuration

Configuration section: `Softalleys:Events:Distributed:GooglePubSub`

Common keys:
- `ProjectId` (string): GCP project id
- `TopicId` (string): Pub/Sub topic (default `events`)
- `SubscribePath` (string): Minimal API path for push (default `/google-pubsub/receive`)
- `SubscriptionId` (string): subscription name (required for pull; used by tooling/provisioning)
- `RequireJwtValidation` (bool, default true): validate Bearer token on push requests
- `Audience` (string): expected audience for JWT
- `Issuer` (string): expected issuer for JWT (e.g., `https://accounts.google.com`)
- `JwksEndpoint` (string): optional JWKS if using custom issuer
- `EnablePullSubscriber` (bool): enable/disable pull subscriber background service
- `AutoProvisionTopic` (bool): dev convenience; topic is already ensured on publish
- `AutoProvisionSubscription` (bool): dev convenience for pull; create sub if missing
- `PushEndpoint` (string): target URL when provisioning a push subscription (dev/emulator)
- `AckDeadlineSeconds` (int): ack deadline used when creating subscriptions (dev)

Environment variables mapping uses `__` as separator, for example:

```text
Softalleys__Events__Distributed__GooglePubSub__ProjectId=local-project
Softalleys__Events__Distributed__GooglePubSub__TopicId=events
Softalleys__Events__Distributed__GooglePubSub__SubscriptionId=pong-sub
Softalleys__Events__Distributed__GooglePubSub__EnablePullSubscriber=true
```

Emulator:
- Set `PUBSUB_EMULATOR_HOST=host:port` to make clients use the emulator.
- For push in Docker, `PushEndpoint` typically points to another container by name (e.g., `http://pong:5187/google-pubsub/receive`).

---

## Cloud Run guidance (production)

- Prefer push subscriptions (or Eventarc Pub/Sub triggers) for Cloud Run; it aligns with request-driven scaling.
- Use HTTPS endpoints and OIDC tokens:
  - Configure the push subscription with a service account that has `Cloud Run Invoker` on your service
  - Validate JWT: Issuer `https://accounts.google.com`; Audience your Cloud Run URL
- Make handlers idempotent; Pub/Sub is at-least-once
- Use Dead Letter Queues and retry policies on the subscription for poison messages
- Provision topics/subscriptions with IaC; keep auto-provision flags off in production

Pull mode on Cloud Run is possible but less common; you’ll manage lifecycle and scaling carefully. Consider GKE/GCE for long-lived pull workers.

---

## FAQ

Q: Can a subscription push to multiple endpoints?
- No. One push endpoint per subscription. Use multiple subscriptions to fan-out.

Q: Does the library auto-provision resources?
- Topic existence is ensured by the publisher on first publish (helpful for emulator/dev).
- Pull subscriptions can be auto-created if `EnablePullSubscriber=true` and `AutoProvisionSubscription=true`.
- Push subscriptions are typically created with IaC or tooling; configure `PushEndpoint` only for dev/emulator flows when you create them yourself.

Q: How do I map event names to types?
- Use `dist.Naming.UseKebabCase().Map<YourEvent>("your-event", version)` when configuring the distributed events builder.

---

## End-to-end flow recap

1) Sender publishes a strongly-typed event via `IEventBus`.
2) Transport serializes and publishes to Pub/Sub topic.
3) Receiver gets the event by:
   - Push: Pub/Sub POSTs to `SubscribePath`, receiver validates auth (prod) and processes.
   - Pull: Background subscriber pulls and dispatches to your handler.
4) Handler executes business logic; on success the message is acked (push: 2xx; pull: Ack).

---

Happy shipping event-driven services on Pub/Sub!
