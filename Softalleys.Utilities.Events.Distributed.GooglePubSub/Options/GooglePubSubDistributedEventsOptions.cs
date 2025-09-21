namespace Softalleys.Utilities.Events.Distributed.GooglePubSub.Options;

public sealed class GooglePubSubDistributedEventsOptions
{
    // Publishing
    public string? ProjectId { get; set; }
    public string TopicId { get; set; } = "events";

    // Receiving via HTTP minimal APIs
    public string SubscribePath { get; set; } = "/google-pubsub/receive"; // must match the push endpoint configured in GCP
    public string? SubscriptionId { get; set; } // used for both push (provisioning) and pull

    // JWT validation
    public bool RequireJwtValidation { get; set; } = true;
    public string? Audience { get; set; }
    public string? Issuer { get; set; }
    public string? JwksEndpoint { get; set; }
    public Func<string, Task<bool>>? CustomJwtValidator { get; set; }

    // Pull subscriber
    public bool EnablePullSubscriber { get; set; } = false;

    // Provisioning (dev/emulator convenience; in prod this is often managed by IaC)
    public bool AutoProvisionTopic { get; set; } = false;
    public bool AutoProvisionSubscription { get; set; } = false; // if PushEndpoint is set => push sub, else pull sub
    public string? PushEndpoint { get; set; } // e.g., Cloud Run URL in prod, or http://service:port/path in docker
    public int AckDeadlineSeconds { get; set; } = 10;
}
