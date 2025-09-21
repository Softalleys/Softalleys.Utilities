using System.Text.Json;

namespace Softalleys.Utilities.Events.Distributed;

public sealed record DistributedEventEnvelopeRaw(JsonElement Data, DistributedEventMetadata Meta);
