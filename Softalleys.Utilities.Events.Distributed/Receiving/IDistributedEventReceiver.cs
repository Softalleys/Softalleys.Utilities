namespace Softalleys.Utilities.Events.Distributed.Receiving;

public interface IDistributedEventReceiver
{
    Task<InboundProcessOutcome> ProcessAsync(DistributedInboundMessage message, CancellationToken cancellationToken = default);
}

public sealed record DistributedInboundMessage(
    ReadOnlyMemory<byte> Payload,
    string? ContentType = null,
    string? EventName = null,
    int? Version = null,
    IReadOnlyDictionary<string, string>? Headers = null,
    string? Source = null);

public enum InboundProcessOutcome
{
    Success,
    Retry,
    DeadLetter
}
