namespace Softalleys.Utilities.Events.Distributed;

public sealed record DistributedEventEnvelope<T>(T Data, DistributedEventMetadata Meta) where T : IEvent;

public sealed class DistributedEventMetadata
{
    public string EventId { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // CLR full name
    public int Version { get; set; } = 1;
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
    public string? CorrelationId { get; set; }
    public string? CausationId { get; set; }
    public string? TenantId { get; set; }
    public string? Source { get; set; }
    public IReadOnlyDictionary<string, string> Headers { get; init; } = new Dictionary<string, string>();
}
