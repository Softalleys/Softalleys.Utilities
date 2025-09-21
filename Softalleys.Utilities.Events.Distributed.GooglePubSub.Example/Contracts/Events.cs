using Softalleys.Utilities.Events;

namespace Softalleys.Utilities.Events.Distributed.GooglePubSub.Example.Contracts;

public sealed class PingRequested : IEvent
{
    public string Message { get; init; } = string.Empty;
}

public sealed class PongReceived : IEvent
{
    public string Message { get; init; } = string.Empty;
    public DateTimeOffset ReceivedAt { get; init; } = DateTimeOffset.UtcNow;
}
