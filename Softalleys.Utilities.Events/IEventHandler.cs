namespace Softalleys.Utilities.Events;

/// <summary>
/// Defines a scoped event handler for a specific event type.
/// Handlers implementing this interface are registered as scoped services and 
/// are resolved within the current DI scope.
/// </summary>
/// <typeparam name="TEvent">The type of event this handler processes. Must implement <see cref="IEvent"/>.</typeparam>
public interface IEventHandler<TEvent> where TEvent : IEvent
{
    /// <summary>
    /// Handles the specified event asynchronously.
    /// </summary>
    /// <param name="eventData">The event data to handle.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous event handling operation.</returns>
    Task HandleAsync(TEvent eventData, CancellationToken cancellationToken = default);
}
