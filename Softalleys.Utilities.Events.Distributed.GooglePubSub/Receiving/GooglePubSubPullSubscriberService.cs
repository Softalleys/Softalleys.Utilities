using Google.Api.Gax;
using Google.Cloud.PubSub.V1;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Softalleys.Utilities.Events.Distributed.GooglePubSub.Options;
using Softalleys.Utilities.Events.Distributed.Receiving;

namespace Softalleys.Utilities.Events.Distributed.GooglePubSub.Receiving;

internal sealed class GooglePubSubPullSubscriberService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly IOptions<GooglePubSubDistributedEventsOptions> _options;
    private readonly ILogger<GooglePubSubPullSubscriberService> _logger;

    public GooglePubSubPullSubscriberService(IServiceProvider services, IOptions<GooglePubSubDistributedEventsOptions> options, ILogger<GooglePubSubPullSubscriberService> logger)
    {
        _services = services;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var o = _options.Value;
        if (!o.EnablePullSubscriber)
        {
            _logger.LogDebug("Pull subscriber disabled");
            return;
        }
        if (string.IsNullOrWhiteSpace(o.ProjectId) || string.IsNullOrWhiteSpace(o.SubscriptionId))
        {
            _logger.LogWarning("Pull subscriber requires ProjectId and SubscriptionId");
            return;
        }

        var subName = SubscriptionName.FromProjectSubscription(o.ProjectId, o.SubscriptionId);
    SubscriberServiceApiClient? subAdmin = null;
    SubscriberClient? subscriber = null;
        try
        {
            subAdmin = await new SubscriberServiceApiClientBuilder { EmulatorDetection = EmulatorDetection.EmulatorOrProduction }.BuildAsync(stoppingToken);
            try
            {
                // Ensure subscription exists when provisioning flags are set; otherwise just rely on existing
                if (o.AutoProvisionSubscription)
                {
                    var topicName = TopicName.FromProjectTopic(o.ProjectId, o.TopicId);
                    await subAdmin.CreateSubscriptionAsync(subName, topicName, pushConfig: null, ackDeadlineSeconds: o.AckDeadlineSeconds, cancellationToken: stoppingToken);
                }
            }
            catch (RpcException ex) when (ex.Status.StatusCode == StatusCode.AlreadyExists) { }

            subscriber = await new SubscriberClientBuilder { SubscriptionName = subName, EmulatorDetection = EmulatorDetection.EmulatorOrProduction }.BuildAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize SubscriberClient");
            return;
        }

        _logger.LogInformation("Starting pull subscriber for {Subscription}", subName);
        var processingTask = subscriber!.StartAsync(async (PubsubMessage msg, CancellationToken ct) =>
        {
            try
            {
                var data = msg.Data.ToByteArray();
                var headers = msg.Attributes.ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);
                var contentType = headers.TryGetValue("contentType", out var ctHeader) ? ctHeader : "application/json";

                using var scope = _services.CreateScope();
                var receiver = scope.ServiceProvider.GetRequiredService<IDistributedEventReceiver>();
                var outcome = await receiver.ProcessAsync(new DistributedInboundMessage(data, contentType, null, null, headers, "google-pubsub"), ct);
                return outcome == InboundProcessOutcome.Success ? SubscriberClient.Reply.Ack : SubscriberClient.Reply.Nack;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Pub/Sub message");
                return SubscriberClient.Reply.Nack;
            }
        });

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // expected on stop
        }
        finally
        {
            try { await subscriber.StopAsync(CancellationToken.None); } catch { }
            try { await processingTask; } catch { }
            // SubscriberClient has no ShutdownAsync; StopAsync + awaiting processingTask is sufficient
        }
    }
}
