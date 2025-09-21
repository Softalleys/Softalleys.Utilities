using System.Text.Json;

namespace Softalleys.Utilities.Events.Distributed.Serialization;

public interface IEventSerializer
{
    byte[] Serialize<TEvent>(DistributedEventEnvelope<TEvent> envelope) where TEvent : IEvent;
    DistributedEventEnvelopeRaw Deserialize(ReadOnlySpan<byte> payload, string? contentType = null);
}

public sealed class SystemTextJsonEventSerializer : IEventSerializer
{
    public static readonly SystemTextJsonEventSerializer Default = new();
    private readonly JsonSerializerOptions _options;

    public SystemTextJsonEventSerializer(JsonSerializerOptions? options = null)
    {
        _options = options ?? new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public byte[] Serialize<TEvent>(DistributedEventEnvelope<TEvent> envelope) where TEvent : IEvent
        => JsonSerializer.SerializeToUtf8Bytes(envelope, _options);

    public DistributedEventEnvelopeRaw Deserialize(ReadOnlySpan<byte> payload, string? contentType = null)
        => JsonSerializer.Deserialize<DistributedEventEnvelopeRaw>(payload, _options)
           ?? throw new InvalidOperationException("Invalid distributed envelope");
}
