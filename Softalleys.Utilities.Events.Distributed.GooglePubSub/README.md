# Softalleys.Utilities.Events.Distributed.GooglePubSub

Google Pub/Sub transport for `Softalleys.Utilities.Events.Distributed` with Minimal APIs receiver.

Features:
- Publisher to Google Pub/Sub topics
- Minimal API endpoint to receive push subscriptions (configurable path)
- Pluggable JWT validation: built-in basic checks + custom delegate

Program.cs usage:

```csharp
builder.Services
    .AddSoftalleysEvents()
    .AddDistributedEvents(dist =>
    {
        dist.Emit.AllEvents();
        dist.UseGooglePubSub(g => g.Configure(o =>
        {
            o.ProjectId = "my-gcp-project";
            o.TopicId = "events";
            o.SubscribePath = "/.well-known/events/subscribe"; // default
            o.Audience = "my-audience"; // optional
            o.Issuer = "https://issuer.example"; // optional
            o.RequireJwtValidation = true; // default
        }));
    });

var app = builder.Build();

// Map receiver endpoint (default path or override)
app.MapGooglePubSubReceiver();

app.Run();
```

Configuration section: `Softalleys:Events:Distributed:GooglePubSub`.
