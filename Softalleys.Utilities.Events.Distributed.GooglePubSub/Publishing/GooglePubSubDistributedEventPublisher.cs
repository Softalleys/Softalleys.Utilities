using Google.Cloud.PubSub.V1;
using Google.Api.Gax;
using Grpc.Core;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Softalleys.Utilities.Events.Distributed;
using Softalleys.Utilities.Events.Distributed.Naming;
using Softalleys.Utilities.Events.Distributed.Publishing;
using Softalleys.Utilities.Events.Distributed.Serialization;
using Softalleys.Utilities.Events.Distributed.GooglePubSub.Options;

namespace Softalleys.Utilities.Events.Distributed.GooglePubSub.Publishing;

internal sealed class GooglePubSubDistributedEventPublisher : IDistributedEventPublisher
{
    private readonly ILogger<GooglePubSubDistributedEventPublisher> _logger;
    private readonly IOptions<GooglePubSubDistributedEventsOptions> _options;
    private readonly IEventSerializer _serializer;
    private readonly IEventNameResolver _nameResolver;

    public GooglePubSubDistributedEventPublisher(
        ILogger<GooglePubSubDistributedEventPublisher> logger,
        IOptions<GooglePubSubDistributedEventsOptions> options,
        IEventSerializer serializer,
        IEventNameResolver nameResolver)
    {
        _logger = logger;
        _options = options;
        _serializer = serializer;
        _nameResolver = nameResolver;
    }

    public async Task PublishAsync<TEvent>(DistributedEventEnvelope<TEvent> envelope, CancellationToken cancellationToken = default) where TEvent : IEvent
    {
        var o = _options.Value;
        if (string.IsNullOrWhiteSpace(o.ProjectId))
        {
            _logger.LogWarning("Google PubSub ProjectId not configured; skipping publish.");
            return;
        }

        var topicName = TopicName.FromProjectTopic(o.ProjectId, o.TopicId);
        _logger.LogDebug("Preparing to publish to PubSub topic: {Topic}. EmulatorHost={EmuHost}", topicName.ToString(), Environment.GetEnvironmentVariable("PUBSUB_EMULATOR_HOST"));
        // Ensure topic exists (helpful when using emulator or fresh projects)
        try
        {
            var admin = await new PublisherServiceApiClientBuilder { EmulatorDetection = EmulatorDetection.EmulatorOrProduction }.BuildAsync(cancellationToken);
            try { await admin.CreateTopicAsync(topicName, cancellationToken: cancellationToken); }
            catch (RpcException ex) when (ex.Status.StatusCode == StatusCode.AlreadyExists) { }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Could not ensure topic exists; proceeding to publish anyway");
        }
        var publisher = await new PublisherClientBuilder
        {
            TopicName = topicName,
            EmulatorDetection = EmulatorDetection.EmulatorOrProduction
        }.BuildAsync(cancellationToken);

        var data = _serializer.Serialize(envelope);
        _logger.LogDebug("Serialized event {Name} v{Version} to {Bytes} bytes", envelope.Meta.Name, envelope.Meta.Version, data.Length);
        var message = new PubsubMessage
        {
            Data = ByteString.CopyFrom(data)
        };
        message.Attributes["eventName"] = envelope.Meta.Name;
        message.Attributes["eventVersion"] = envelope.Meta.Version.ToString();
        message.Attributes["contentType"] = "application/json";
        if (envelope.Meta.Headers != null)
        {
            foreach (var kv in envelope.Meta.Headers)
                message.Attributes[kv.Key] = kv.Value;
        }

        _logger.LogDebug("Publishing message with {AttrCount} attributes to {Topic}", message.Attributes.Count, topicName.ToString());
        var msgId = await publisher.PublishAsync(message);
        _logger.LogDebug("PublishAsync returned messageId={MessageId}", msgId);
        try
        {
            await publisher.ShutdownAsync(TimeSpan.FromSeconds(5));
            _logger.LogDebug("PublisherClient shutdown completed (flushed)");
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "PublisherClient shutdown encountered an issue");
        }
        _logger.LogDebug("Published event {Event} v{Version} to Google PubSub topic {Topic}", envelope.Meta.Name, envelope.Meta.Version, o.TopicId);
    }
}
