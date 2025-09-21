using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Softalleys.Utilities.Events;
using Softalleys.Utilities.Events.Distributed.Configuration;
using Softalleys.Utilities.Events.Distributed.GooglePubSub;
using Softalleys.Utilities.Events.Distributed.GooglePubSub.Options;
using Softalleys.Utilities.Events.Distributed.GooglePubSub.Example.Contracts;
using Google.Cloud.PubSub.V1;
using Google.Api.Gax;

var builder = Host.CreateApplicationBuilder(args);

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
        dist.Emit.AllEvents();
        // Ensure the process waits for distributed publish to complete (important for short-lived console app)
        dist.Emit.RequireDistributedSuccess();
        dist.Serialization.UseSystemTextJson();
        dist.Naming.UseKebabCase().Map<PingRequested>("ping-requested", 1);
        dist.UseGooglePubSub(g => g.Configure(o =>
        {
            // bind from configuration if present
            builder.Configuration.GetSection("Softalleys:Events:Distributed:GooglePubSub").Bind(o);
            o.RequireJwtValidation = false; // publisher side
            if (string.IsNullOrWhiteSpace(o.ProjectId)) o.ProjectId = "local-project";
            if (string.IsNullOrWhiteSpace(o.TopicId)) o.TopicId = "events";
            // For local dev, optionally provision topic automatically (emulator)
            o.AutoProvisionTopic = o.AutoProvisionTopic || !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("PUBSUB_EMULATOR_HOST"));
        }));
    });

var host = builder.Build();

// Command: publish "message"
var message = args.Length > 1 && args[0].Equals("publish", StringComparison.OrdinalIgnoreCase)
    ? string.Join(" ", args.Skip(1))
    : "hello from ping";

// No Pub/Sub admin/provisioning code here. The library can auto-provision when configured.

await using var scope = host.Services.CreateAsyncScope();
var bus = scope.ServiceProvider.GetRequiredService<IEventBus>();
await bus.PublishAsync(new PingRequested { Message = message });

Console.WriteLine($"Published PingRequested: '{message}'");
