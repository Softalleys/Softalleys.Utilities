using System;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Softalleys.Utilities.Events.Distributed;
using Softalleys.Utilities.Events.Distributed.Configuration;
using Softalleys.Utilities.Events.Distributed.Publishing;
using Softalleys.Utilities.Events.Distributed.RabbitMQ;
using Softalleys.Utilities.Events.Distributed.RabbitMQ.Options;
using Softalleys.Utilities.Events.Distributed.RabbitMQ.Routing;
using Softalleys.Utilities.Events;
using Xunit;

namespace Softalleys.Utilities.Events.Tests.Distributed;

public class RabbitMqTransportTests
{
    private sealed record TestEvent(string Value) : IEvent;

    [Fact]
    public void UseRabbitMq_registers_publisher_and_resolver()
    {
    var services = new ServiceCollection();
    services.AddSingleton<Microsoft.Extensions.Configuration.IConfiguration>(new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build());
        services.AddLogging();
        services.AddSoftalleysEvents();
        services.AddDistributedEvents(dist =>
        {
            dist.Emit.AllEvents();
            dist.UseRabbitMq(rb => rb.Configure(o =>
            {
                o.Exchange = "x";
                o.QueueName = "q";
            }));
        });

        var sp = services.BuildServiceProvider();
    var publishers = sp.GetServices<IDistributedEventPublisher>().ToList();
    // At least one publisher should be registered in addition to the default NoOpPublisher
    Assert.True(publishers.Count >= 1);

        var resolver = sp.GetService<IRabbitMqRoutingResolver>();
        Assert.NotNull(resolver);
        var opts = sp.GetRequiredService<IOptions<RabbitMqDistributedEventsOptions>>().Value;
        Assert.Equal("x", opts.Exchange);
        Assert.Equal("q", opts.QueueName);
    }

    [Fact]
    public void RoutingResolver_uses_template_and_per_event_override()
    {
        var services = new ServiceCollection();
        services.Configure<RabbitMqDistributedEventsOptions>(o =>
        {
            o.Exchange = "ex-default";
            o.RoutingKeyTemplate = "{name}.v{version}";
            o.Events["test-event"] = new() { RoutingKey = "custom.key", Exchange = "ex-custom", Mandatory = true };
        });

    services.AddSingleton<IRabbitMqRoutingResolver, Softalleys.Utilities.Events.Distributed.RabbitMQ.Routing.RabbitMqRoutingResolver>();

        var sp = services.BuildServiceProvider();
        var resolver = sp.GetRequiredService<IRabbitMqRoutingResolver>();

        var metaDefault = new DistributedEventMetadata
        {
            EventId = Guid.NewGuid().ToString("N"),
            Name = "another-event",
            Type = "type",
            Version = 1,
            OccurredAt = DateTimeOffset.UtcNow
        };
        var (ex1, rk1, man1) = resolver.Resolve(metaDefault);
        Assert.Equal("ex-default", ex1);
        Assert.Equal("another-event.v1", rk1);
        Assert.False(man1);

        var metaOverride = new DistributedEventMetadata
        {
            EventId = Guid.NewGuid().ToString("N"),
            Name = "test-event",
            Type = "type",
            Version = 2,
            OccurredAt = DateTimeOffset.UtcNow
        };
        var (ex2, rk2, man2) = resolver.Resolve(metaOverride);
        Assert.Equal("ex-custom", ex2);
        Assert.Equal("custom.key", rk2);
        Assert.True(man2);
    }
}
