# Softalleys.Utilities.Events.Distributed.GooglePubSub.Example

Run two apps, Ping (publisher) and Pong (subscriber), using Google Pub/Sub locally via the emulator — no Google Cloud account or credentials required.

- docker-compose runs the Pub/Sub emulator, Pong, and Ping together
- Ping publishes a `PingRequested` event to topic `events`
- Pong subscribes (pull) from `events` and logs received events
- Pong also exposes a Minimal API endpoint to accept Pub/Sub push (dev-only simulation)

## Prerequisites

- Docker Desktop
- .NET 8/9 SDK (for local builds; containers build with .NET SDK 9)
- (Optional) gcloud CLI for manual topic/subscription management

## Quick start (Docker Compose)

From the repo root:

```powershell
cd Softalleys.Utilities.Events.Distributed.GooglePubSub.Example
docker compose up --build
```

What happens:

- Pub/Sub emulator container starts and listens on port 8681
- Pong starts, auto-creates topic/subscription (if missing), and begins pulling
- Ping runs and publishes a `PingRequested` message ("HelloFromDocker")

Verify logs:

```powershell
docker logs pong --since 5m
docker logs ping --since 5m
```

You should see Pong report it started and Ping report a published message.

To stop:

```powershell
docker compose down -v
```

## Configuration

Defaults used by compose (overridable via environment variables):

- Softalleys__Events__Distributed__GooglePubSub__ProjectId=local-project
- Softalleys__Events__Distributed__GooglePubSub__TopicId=events
- Pong__SubscriptionId=pong-sub

Emulator env wired inside containers:

- PUBSUB_EMULATOR_HOST=pubsub:8681
- CLOUDSDK_API_ENDPOINT_OVERRIDES_PUBSUB=http://pubsub:8681/

Note: This emulator image exposes gRPC on 8681 (not 8085). Compose, apps, and healthchecks are configured accordingly.

## Local (non-Docker) run

If you want to run without Docker, start an emulator and set env vars in your shell:

```powershell
$env:PUBSUB_EMULATOR_HOST = "localhost:8681"
$env:CLOUDSDK_API_ENDPOINT_OVERRIDES_PUBSUB = "http://localhost:8681/"   # optional, for gcloud commands
```

Then run each app from the example folder:

```powershell
cd Pong
dotnet run

cd ../Ping
dotnet run -- publish "hello from ping"
```

## Simulate push delivery (optional)

Pong exposes `/.well-known/events/subscribe`. The emulator doesn't push, but you can POST a Pub/Sub-like payload for testing:

```powershell
$payload = [Text.Encoding]::UTF8.GetBytes('{"hello":"world"}')
$data = [Convert]::ToBase64String($payload)
$body = '{"message":{"data":"'+$data+'","attributes":{"contentType":"application/json"}},"subscription":"projects/test/subscriptions/dummy"}'
Invoke-RestMethod -Method Post -Uri http://localhost:5187/.well-known/events/subscribe -ContentType application/json -Body $body
```

## Troubleshooting

- Emulator not ready / connection refused: it can take a few seconds after container start; Pong includes a retry loop.
- NotFound on publish: first run race conditions; Pong/Ping attempt to auto-create topic/subscription.
- Messages not arriving: confirm the same ProjectId/TopicId/SubscriptionId are used by both apps and the emulator.

—

Happy eventing!