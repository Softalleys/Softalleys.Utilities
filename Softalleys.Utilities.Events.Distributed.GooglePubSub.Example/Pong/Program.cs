using Google.Api.Gax.ResourceNames;
using Google.Api.Gax;
using Google.Cloud.PubSub.V1;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Softalleys.Utilities.Events;
using Softalleys.Utilities.Events.Distributed.Configuration;
using Softalleys.Utilities.Events.Distributed.GooglePubSub;
using Softalleys.Utilities.Events.Distributed.GooglePubSub.Options;
// using Softalleys.Utilities.Events.Distributed.GooglePubSub.Receiving; // push receiver not used in this simplified example
using Softalleys.Utilities.Events.Distributed.GooglePubSub.Example.Contracts;
using Softalleys.Utilities.Events.Distributed.Receiving;
using Softalleys.Utilities.Events.Distributed.GooglePubSub.Receiving;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);
builder.Logging.AddFilter("Softalleys.Utilities", LogLevel.Debug)
               .AddFilter("Softalleys.Utilities.Events.Distributed", LogLevel.Debug)
               .AddFilter("Softalleys.Utilities.Events.Distributed.GooglePubSub", LogLevel.Debug);
builder.Configuration.AddJsonFile("appsettings.json", optional: true)
                     .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
                     .AddEnvironmentVariables();

builder.Services
    .AddSoftalleysEvents(typeof(PingRequested).Assembly)
    .AddDistributedEvents(dist =>
    {
        // Do NOT emit distributed events from the subscriber to avoid feedback loops
        // (leave default NoneEventsFilter). Only set serialization and naming/mapping.
        dist.Serialization.UseSystemTextJson();
        dist.Naming.UseKebabCase().Map<PingRequested>("ping-requested", 1);
        dist.UseGooglePubSub(g => g.Configure(o =>
        {
            // bind from configuration if present
            builder.Configuration.GetSection("Softalleys:Events:Distributed:GooglePubSub").Bind(o);
            // Emulator push doesn't add Authorization header; disable JWT validation here
            o.RequireJwtValidation = false;
            if (string.IsNullOrWhiteSpace(o.ProjectId)) o.ProjectId = "local-project";
            if (string.IsNullOrWhiteSpace(o.TopicId)) o.TopicId = "events";
            // For local/dev, auto-provision topic and a push subscription to this service
            var isEmulator = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("PUBSUB_EMULATOR_HOST"));
            if (isEmulator)
            {
                o.AutoProvisionTopic = true;
                o.AutoProvisionSubscription = true;
                o.SubscriptionId ??= builder.Configuration["Pong:SubscriptionId"] ?? "pong-sub";
            }
        }));
    });

builder.Services.AddScoped<IEventHandler<PingRequested>, PingRequestedHandler>();

var app = builder.Build();
// Only expose a simple health endpoint; receiving is handled by push (and/or pull if enabled)
app.MapGet("/health", () => Results.Ok("ok"));

// Map the receive endpoint for push subscriptions if needed
app.MapGooglePubSubReceiver(); // default route from options (/google-pubsub/receive) 

await app.RunAsync();

public sealed class PingRequestedHandler : IEventHandler<PingRequested>
{
    public Task HandleAsync(PingRequested eventData, CancellationToken cancellationToken = default)
    {
        // Keep it simple: just print the incoming message to the console
        Console.WriteLine($"[PONG] Received PingRequested: {eventData.Message}");
        return Task.CompletedTask;
    }
}
