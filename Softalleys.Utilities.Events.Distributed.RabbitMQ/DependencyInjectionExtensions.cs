using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Softalleys.Utilities.Events.Distributed.Configuration;
using Softalleys.Utilities.Events.Distributed.RabbitMQ.Options;
using Softalleys.Utilities.Events.Distributed.RabbitMQ.Publishing;
using Softalleys.Utilities.Events.Distributed.RabbitMQ.Receiving;
using Softalleys.Utilities.Events.Distributed.Publishing;
using Softalleys.Utilities.Events.Distributed.RabbitMQ.Routing;

namespace Softalleys.Utilities.Events.Distributed.RabbitMQ;

public static class DependencyInjectionExtensions
{
    public static IDistributedEventsBuilder UseRabbitMq(this IDistributedEventsBuilder builder, Action<RabbitMqBuilder>? configure = null)
    {
        var services = builder.Services;

        // Bind options from configuration/environment if available
        services.AddOptions<RabbitMqDistributedEventsOptions>()
            .BindConfiguration("Softalleys:Events:Distributed:RabbitMQ")
            .ValidateOnStart();

        // Allow further code-based configuration
        var rb = new RabbitMqBuilder(services);
        configure?.Invoke(rb);

    services.TryAddSingleton<IRabbitMqRoutingResolver, RabbitMqRoutingResolver>();
    services.TryAddEnumerable(ServiceDescriptor.Singleton<IDistributedEventPublisher, RabbitMqDistributedEventPublisher>());
        services.AddHostedService<RabbitMqSubscriberHostedService>();

        return builder;
    }
}

public sealed class RabbitMqBuilder
{
    internal RabbitMqBuilder(IServiceCollection services) => Services = services;
    internal IServiceCollection Services { get; }

    public RabbitMqBuilder Configure(Action<RabbitMqDistributedEventsOptions> configure)
    {
        Services.Configure(configure);
        return this;
    }

    public RabbitMqBuilder Configure(IConfiguration configuration)
    {
        Services.Configure<RabbitMqDistributedEventsOptions>(configuration.GetSection("Softalleys:Events:Distributed:RabbitMQ"));
        return this;
    }
}
