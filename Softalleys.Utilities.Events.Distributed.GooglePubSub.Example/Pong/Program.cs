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
    });

// Configure Google Pub/Sub options for the pull subscriber (without registering the publisher)
builder.Services.Configure<GooglePubSubDistributedEventsOptions>(o =>
{
    builder.Configuration.GetSection("Softalleys:Events:Distributed:GooglePubSub").Bind(o);
    if (string.IsNullOrWhiteSpace(o.ProjectId)) o.ProjectId = "local-project";
    if (string.IsNullOrWhiteSpace(o.TopicId)) o.TopicId = "events";
});

builder.Services.AddHostedService<PullSubscriberHostedService>();
builder.Services.AddScoped<IEventHandler<PingRequested>, PingRequestedHandler>();

var app = builder.Build();
// Only expose a simple health endpoint; receiving is done via pull subscriber below
app.MapGet("/health", () => Results.Ok("ok"));
await app.RunAsync();

public sealed class PullSubscriberHostedService(
    ILogger<PullSubscriberHostedService> logger,
    IHostApplicationLifetime lifetime,
    Microsoft.Extensions.Options.IOptions<GooglePubSubDistributedEventsOptions> opts,
    IServiceProvider serviceProvider)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var o = opts.Value;
        if (string.IsNullOrWhiteSpace(o.ProjectId)) o.ProjectId = "local-project";
        var projectId = o.ProjectId!;
        var topicId = o.TopicId ?? "events";
        var subscriptionId = Environment.GetEnvironmentVariable("Pong__SubscriptionId") ?? "pong-sub";

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var publisher = await new PublisherServiceApiClientBuilder
                {
                    EmulatorDetection = EmulatorDetection.EmulatorOrProduction
                }.BuildAsync(stoppingToken);
                var subscriber = await new SubscriberServiceApiClientBuilder
                {
                    EmulatorDetection = EmulatorDetection.EmulatorOrProduction
                }.BuildAsync(stoppingToken);

                var topicName = TopicName.FromProjectTopic(projectId, topicId);
                try { await publisher.CreateTopicAsync(topicName, cancellationToken: stoppingToken); }
                catch (Grpc.Core.RpcException ex) when (ex.Status.StatusCode == Grpc.Core.StatusCode.AlreadyExists) { }

                var subscriptionName = SubscriptionName.FromProjectSubscription(projectId, subscriptionId);
                try { await subscriber.CreateSubscriptionAsync(subscriptionName, topicName, pushConfig: null, ackDeadlineSeconds: 10, cancellationToken: stoppingToken); }
                catch (Grpc.Core.RpcException ex) when (ex.Status.StatusCode == Grpc.Core.StatusCode.AlreadyExists) { }

                var client = await new SubscriberClientBuilder
                {
                    SubscriptionName = subscriptionName,
                    EmulatorDetection = EmulatorDetection.EmulatorOrProduction
                }.BuildAsync(stoppingToken);
                logger.LogInformation("Pong subscriber started. Project={Project} Topic={Topic} Subscription={Sub}", projectId, topicId, subscriptionId);

                await client.StartAsync(async (msg, ct) =>
                {
                    try
                    {
                        using var scope = serviceProvider.CreateScope();
                        var receiver = scope.ServiceProvider.GetRequiredService<IDistributedEventReceiver>();
                        var headers = msg.Attributes?.ToDictionary(kv => kv.Key, kv => kv.Value) ?? new Dictionary<string, string>();
                        var contentType = headers.TryGetValue("contentType", out var ctHeader) ? ctHeader : "application/json";
                        var outcome = await receiver.ProcessAsync(new DistributedInboundMessage(msg.Data.Memory, contentType, headers.TryGetValue("eventName", out var en) ? en : null,
                            headers.TryGetValue("eventVersion", out var ev) && int.TryParse(ev, out var v) ? v : null, headers, "google-pubsub-pull"), ct);
                        if (outcome == InboundProcessOutcome.Success) return SubscriberClient.Reply.Ack;
                        if (outcome == InboundProcessOutcome.DeadLetter) return SubscriberClient.Reply.Ack; // ack to drop
                        return SubscriberClient.Reply.Nack; // retry
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Processing failed");
                        return SubscriberClient.Reply.Nack;
                    }
                });

                lifetime.ApplicationStopped.Register(async () => await client.StopAsync(CancellationToken.None));

                // Block until cancellation; if cancelled, loop exits gracefully
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Pub/Sub emulator not ready. Retrying in 2s...");
                try { await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken); } catch { }
            }
        }
    }
}

public sealed class PingRequestedHandler : IEventHandler<PingRequested>
{
    public Task HandleAsync(PingRequested eventData, CancellationToken cancellationToken = default)
    {
        // Keep it simple: just print the incoming message to the console
        Console.WriteLine($"[PONG] Received PingRequested: {eventData.Message}");
        return Task.CompletedTask;
    }
}
