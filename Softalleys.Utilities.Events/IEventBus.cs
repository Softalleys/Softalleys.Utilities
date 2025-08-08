namespace Softalleys.Utilities.Events;

/// <summary>
/// Defines the contract for publishing events to registered handlers.
/// The event bus manages the lifecycle and execution of event handlers based on their registration scope.
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publishes an event to all registered handlers asynchronously.
    /// Handlers are executed in the following order:
    /// 1. Pre-processing singleton handlers
    /// 2. Pre-processing scoped handlers  
    /// 3. Main singleton handlers
    /// 4. Main scoped handlers
    /// 5. Post-processing singleton handlers
    /// 6. Post-processing scoped handlers
    /// </summary>
    /// <typeparam name="TEvent">The type of event to publish. Must implement <see cref="IEvent"/>.</typeparam>
    /// <param name="eventData">The event data to publish to handlers.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous publish operation.</returns>
    Task PublishAsync<TEvent>(TEvent eventData, CancellationToken cancellationToken = default) where TEvent : IEvent;
}
