using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Softalleys.Utilities.Events.Distributed.Configuration;
using Softalleys.Utilities.Events.Distributed.GooglePubSub.Options;
using Softalleys.Utilities.Events.Distributed.GooglePubSub.Publishing;
using Softalleys.Utilities.Events.Distributed.GooglePubSub.Receiving;
using Softalleys.Utilities.Events.Distributed.Publishing;

namespace Softalleys.Utilities.Events.Distributed.GooglePubSub;

public static class DependencyInjectionExtensions
{
    public static IDistributedEventsBuilder UseGooglePubSub(this IDistributedEventsBuilder builder, Action<GooglePubSubBuilder>? configure = null)
    {
        var services = builder.Services;
        services.AddOptions<GooglePubSubDistributedEventsOptions>()
            .BindConfiguration("Softalleys:Events:Distributed:GooglePubSub")
            .ValidateOnStart();

        var gb = new GooglePubSubBuilder(services);
        configure?.Invoke(gb);

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IDistributedEventPublisher, GooglePubSubDistributedEventPublisher>());
        // Optional hosted services: provisioning and pull subscriber
        services.AddHostedService<GooglePubSubPullSubscriberService>();
        return builder;
    }
}

public sealed class GooglePubSubBuilder
{
    internal GooglePubSubBuilder(IServiceCollection services) => Services = services;
    internal IServiceCollection Services { get; }

    public GooglePubSubBuilder Configure(Action<GooglePubSubDistributedEventsOptions> configure)
    {
        Services.Configure(configure);
        return this;
    }

    public GooglePubSubBuilder Configure(IConfiguration configuration)
    {
        Services.Configure<GooglePubSubDistributedEventsOptions>(configuration.GetSection("Softalleys:Events:Distributed:GooglePubSub"));
        return this;
    }
}
