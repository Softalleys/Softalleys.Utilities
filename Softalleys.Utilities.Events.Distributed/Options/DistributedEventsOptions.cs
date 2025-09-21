using Softalleys.Utilities.Events.Distributed.Naming;
using Softalleys.Utilities.Events.Distributed.Serialization;
using Softalleys.Utilities.Events.Distributed.Types;

namespace Softalleys.Utilities.Events.Distributed.Options;

public enum DeliveryMode
{
    LocalFirstEmitAsync,
    LocalAndEmitInParallel,
    RequireDistributedSuccess
}

public interface IEventEmitFilter
{
    bool ShouldEmit(Type eventType);
}

public sealed class AllEventsFilter : IEventEmitFilter
{
    public static readonly AllEventsFilter Instance = new();
    private AllEventsFilter() {}
    public bool ShouldEmit(Type eventType) => true;
}

public sealed class OnlySelectedEventsFilter : IEventEmitFilter
{
    private readonly HashSet<Type> _types = new();
    public void Include(Type type) => _types.Add(type);
    public bool ShouldEmit(Type eventType) => _types.Contains(eventType);
}

public sealed class NoneEventsFilter : IEventEmitFilter
{
    public static readonly NoneEventsFilter Instance = new();
    private NoneEventsFilter() {}
    public bool ShouldEmit(Type eventType) => false;
}

public sealed class ObservabilityOptions
{
    public bool TracingEnabled { get; set; }
    public bool MetricsEnabled { get; set; }
    public string ActivitySourceName { get; set; } = "Softalleys.Events.Distributed";
    public string MeterName { get; set; } = "Softalleys.Events.Distributed";
}

public sealed class DistributedEventsOptions
{
    public DeliveryMode DeliveryMode { get; set; } = DeliveryMode.LocalFirstEmitAsync;
    public IEventEmitFilter EmitFilter { get; set; } = NoneEventsFilter.Instance; // default: emit none unless configured
    public IEventNameResolver NameResolver { get; set; } = new DefaultEventNameResolver();
    public IEventSerializer Serializer { get; set; } = SystemTextJsonEventSerializer.Default;
    public IEventTypeRegistry TypeRegistry { get; set; } = new DefaultEventTypeRegistry();
    public ObservabilityOptions Observability { get; } = new();
}
