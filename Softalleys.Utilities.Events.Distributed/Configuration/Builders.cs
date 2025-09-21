using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Softalleys.Utilities.Events.Distributed.Naming;
using Softalleys.Utilities.Events.Distributed.Options;
using Softalleys.Utilities.Events.Distributed.Serialization;
using Softalleys.Utilities.Events.Distributed.Types;

namespace Softalleys.Utilities.Events.Distributed.Configuration;

public interface IDistributedEventsBuilder
{
    IServiceCollection Services { get; }
    DistributedEventsOptions Options { get; }
    IEmitBuilder Emit { get; }
    INamingBuilder Naming { get; }
    ISerializationBuilder Serialization { get; }
    IObservabilityBuilder Observability { get; }
    IDistributedEventsBuilder RegisterTransport(ITransportPlugin plugin);
}

public interface IEmitBuilder
{
    IEmitBuilder AllEvents();
    IEmitBuilder PublishEvent<TEvent>() where TEvent : IEvent;
    IEmitBuilder RequireDistributedSuccess();
    IEmitBuilder LocalAndEmitInParallel();
    IEmitBuilder LocalFirstEmitAsync();
}

public interface INamingBuilder
{
    INamingBuilder UseFullName(bool includeNamespace);
    INamingBuilder UseKebabCase();
    INamingBuilder UseCamelCase();
    INamingBuilder UsePascalCase();
    INamingBuilder UseNamespacePrefix(string prefix);
    INamingBuilder Map<TEvent>(string name, int version = 1) where TEvent : IEvent;
}

public interface ISerializationBuilder
{
    ISerializationBuilder UseSystemTextJson(Action<System.Text.Json.JsonSerializerOptions>? configure = null);
    ISerializationBuilder UseCustom(IEventSerializer serializer);
    ISerializationBuilder WithTypeRegistry(Action<IEventTypeRegistryConfigurator> cfg);
}

public interface IEventTypeRegistryConfigurator
{
    IEventTypeRegistryConfigurator Map(Type clrType, string name, int version = 1);
    IEventTypeRegistryConfigurator Map<TEvent>(string name, int version = 1) where TEvent : IEvent;
}

public interface IObservabilityBuilder
{
    IObservabilityBuilder EnableTracing(string? sourceName = null);
    IObservabilityBuilder EnableMetrics(string? meterName = null);
    IObservabilityBuilder DisableTracing();
    IObservabilityBuilder DisableMetrics();
}

public interface ITransportPlugin
{
    string Name { get; }
    void Register(IServiceCollection services, DistributedEventsOptions options);
}

internal sealed class DistributedEventsBuilder : IDistributedEventsBuilder, IEmitBuilder, INamingBuilder, ISerializationBuilder, IObservabilityBuilder, IEventTypeRegistryConfigurator
{
    private readonly OnlySelectedEventsFilter _onlyFilter = new();

    public IServiceCollection Services { get; }
    public DistributedEventsOptions Options { get; }

    public IEmitBuilder Emit => this;
    public INamingBuilder Naming => this;
    public ISerializationBuilder Serialization => this;
    public IObservabilityBuilder Observability => this;

    public DistributedEventsBuilder(IServiceCollection services, DistributedEventsOptions options)
    {
        Services = services;
        Options = options;
        // Default to emit none unless user explicitly configures
        Options.EmitFilter = NoneEventsFilter.Instance;
    }

    public IDistributedEventsBuilder RegisterTransport(ITransportPlugin plugin)
    {
        plugin.Register(Services, Options);
        return this;
    }

    // Emit
    public IEmitBuilder AllEvents()
    {
        Options.EmitFilter = AllEventsFilter.Instance;
        return this;
    }

    public IEmitBuilder PublishEvent<TEvent>() where TEvent : IEvent
    {
        _onlyFilter.Include(typeof(TEvent));
        Options.EmitFilter = _onlyFilter;
        return this;
    }

    public IEmitBuilder RequireDistributedSuccess()
    {
        Options.DeliveryMode = DeliveryMode.RequireDistributedSuccess;
        return this;
    }

    public IEmitBuilder LocalAndEmitInParallel()
    {
        Options.DeliveryMode = DeliveryMode.LocalAndEmitInParallel;
        return this;
    }

    public IEmitBuilder LocalFirstEmitAsync()
    {
        Options.DeliveryMode = DeliveryMode.LocalFirstEmitAsync;
        return this;
    }

    // Naming
    private DefaultEventNameResolver _resolver = new();
    public INamingBuilder UseFullName(bool includeNamespace)
    {
        _resolver = new DefaultEventNameResolver(useFullName: includeNamespace, includeNamespace: includeNamespace, @case: NameCase.KebabCase);
        Options.NameResolver = _resolver;
        return this;
    }

    public INamingBuilder UseKebabCase()
    {
        _resolver = new DefaultEventNameResolver(useFullName: false, includeNamespace: false, @case: NameCase.KebabCase);
        Options.NameResolver = _resolver;
        return this;
    }

    public INamingBuilder UseCamelCase()
    {
        _resolver = new DefaultEventNameResolver(useFullName: false, includeNamespace: false, @case: NameCase.CamelCase);
        Options.NameResolver = _resolver;
        return this;
    }

    public INamingBuilder UsePascalCase()
    {
        _resolver = new DefaultEventNameResolver(useFullName: false, includeNamespace: false, @case: NameCase.PascalCase);
        Options.NameResolver = _resolver;
        return this;
    }

    public INamingBuilder UseNamespacePrefix(string prefix)
    {
        _resolver = new DefaultEventNameResolver(useFullName: false, includeNamespace: false, @case: NameCase.KebabCase, prefix: prefix);
        Options.NameResolver = _resolver;
        return this;
    }

    public INamingBuilder Map<TEvent>(string name, int version = 1) where TEvent : IEvent
    {
        _resolver.Map(typeof(TEvent), name, version);
        Options.TypeRegistry.Map(typeof(TEvent), name, version);
        return this;
    }

    // Serialization
    public ISerializationBuilder UseSystemTextJson(Action<System.Text.Json.JsonSerializerOptions>? configure = null)
    {
        var opts = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        };
        configure?.Invoke(opts);
        Options.Serializer = new SystemTextJsonEventSerializer(opts);
        return this;
    }

    public ISerializationBuilder UseCustom(IEventSerializer serializer)
    {
        Options.Serializer = serializer;
        return this;
    }

    public ISerializationBuilder WithTypeRegistry(Action<IEventTypeRegistryConfigurator> cfg)
    {
        cfg(this);
        return this;
    }

    // Registry config
    IEventTypeRegistryConfigurator IEventTypeRegistryConfigurator.Map(Type clrType, string name, int version)
    {
        Options.TypeRegistry.Map(clrType, name, version);
        if (_resolver != null) _resolver.Map(clrType, name, version);
        return this;
    }

    IEventTypeRegistryConfigurator IEventTypeRegistryConfigurator.Map<TEvent>(string name, int version)
        => ((IEventTypeRegistryConfigurator)this).Map(typeof(TEvent), name, version);

    // Observability
    public IObservabilityBuilder EnableTracing(string? sourceName = null)
    {
        Options.Observability.TracingEnabled = true;
        if (!string.IsNullOrWhiteSpace(sourceName)) Options.Observability.ActivitySourceName = sourceName!;
        return this;
    }

    public IObservabilityBuilder EnableMetrics(string? meterName = null)
    {
        Options.Observability.MetricsEnabled = true;
        if (!string.IsNullOrWhiteSpace(meterName)) Options.Observability.MeterName = meterName!;
        return this;
    }

    public IObservabilityBuilder DisableTracing()
    {
        Options.Observability.TracingEnabled = false;
        return this;
    }

    public IObservabilityBuilder DisableMetrics()
    {
        Options.Observability.MetricsEnabled = false;
        return this;
    }
}

public static class DistributedDependencyInjectionExtensions
{
    public static IServiceCollection AddDistributedEvents(this IServiceCollection services, Action<IDistributedEventsBuilder>? configure = null)
    {
        // options
        var options = new DistributedEventsOptions();
    services.AddSingleton<IOptions<DistributedEventsOptions>>(_ => Microsoft.Extensions.Options.Options.Create(options));

        // Defaults
        services.TryAddSingleton(options.NameResolver);
        services.TryAddSingleton(options.Serializer);
        services.TryAddSingleton(options.TypeRegistry);

        // Publishers collection (transports will add implementations)
        services.TryAddEnumerable(ServiceDescriptor.Singleton<Publishing.IDistributedEventPublisher, Publishing.NoOpPublisher>());

        // Receiver
        services.TryAddSingleton<Receiving.IDistributedEventReceiver, Receiving.DistributedEventReceiver>();

        // Decorate IEventBus
        services.DecorateEventBusWithDistributed();

        var builder = new DistributedEventsBuilder(services, options);
        configure?.Invoke(builder);
        return services;
    }

    private static void DecorateEventBusWithDistributed(this IServiceCollection services)
    {
        // Replace IEventBus with a decorator that constructs the inner EventBus directly.
        // This avoids circular resolution and does not require Scrutor.
        Microsoft.Extensions.DependencyInjection.Extensions.ServiceCollectionDescriptorExtensions.Replace(
            services,
            ServiceDescriptor.Scoped<IEventBus>(sp =>
            {
                var inner = ActivatorUtilities.CreateInstance<EventBus>(sp);
                var publishers = sp.GetServices<Publishing.IDistributedEventPublisher>();
                var options = sp.GetRequiredService<IOptions<DistributedEventsOptions>>();
                var namer = sp.GetRequiredService<Naming.IEventNameResolver>();
                var serializer = sp.GetRequiredService<Serialization.IEventSerializer>();
                var logger = sp.GetService<Microsoft.Extensions.Logging.ILogger<Publishing.DistributedEventBusDecorator>>();
                return new Publishing.DistributedEventBusDecorator(inner, publishers, options, namer, serializer, logger);
            }));
    }
}
