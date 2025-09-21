using Softalleys.Utilities.Events.Distributed;
using Softalleys.Utilities.Events.Distributed.RabbitMQ.Options;
using Microsoft.Extensions.Options;

namespace Softalleys.Utilities.Events.Distributed.RabbitMQ.Routing;

public interface IRabbitMqRoutingResolver
{
    (string Exchange, string RoutingKey, bool Mandatory) Resolve(DistributedEventMetadata meta);
}

public sealed class RabbitMqRoutingResolver : IRabbitMqRoutingResolver
{
    private readonly IOptions<RabbitMqDistributedEventsOptions> _options;

    public RabbitMqRoutingResolver(IOptions<RabbitMqDistributedEventsOptions> options)
    {
        _options = options;
    }

    public (string Exchange, string RoutingKey, bool Mandatory) Resolve(DistributedEventMetadata meta)
    {
        var o = _options.Value;
        var evtCfg = o.GetEventConfigOrDefault(meta.Name);
        var exchange = evtCfg.Exchange ?? o.Exchange;
        var routingKey = evtCfg.RoutingKey ?? o.RoutingKeyTemplate
            .Replace("{name}", meta.Name, StringComparison.Ordinal)
            .Replace("{version}", meta.Version.ToString(System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal);
        var mandatory = evtCfg.Mandatory ?? o.Mandatory;
        return (exchange, routingKey, mandatory);
    }
}
