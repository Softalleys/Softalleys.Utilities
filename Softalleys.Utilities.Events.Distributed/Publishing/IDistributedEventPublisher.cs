namespace Softalleys.Utilities.Events.Distributed.Publishing;

public interface IDistributedEventPublisher
{
    Task PublishAsync<TEvent>(DistributedEventEnvelope<TEvent> envelope, CancellationToken cancellationToken = default) where TEvent : IEvent;
}
