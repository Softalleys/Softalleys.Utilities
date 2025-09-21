namespace Softalleys.Utilities.Events.Distributed.GooglePubSub.Options;

public sealed class GooglePubSubDistributedEventsOptions
{
    // Publishing
    public string? ProjectId { get; set; }
    public string TopicId { get; set; } = "events";

    // Receiving via HTTP minimal APIs
    public string SubscribePath { get; set; } = "/.well-known/events/subscribe";

    // JWT validation
    public bool RequireJwtValidation { get; set; } = true;
    public string? Audience { get; set; }
    public string? Issuer { get; set; }
    public string? JwksEndpoint { get; set; }
    public Func<string, Task<bool>>? CustomJwtValidator { get; set; }
}
