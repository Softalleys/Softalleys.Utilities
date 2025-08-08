namespace Softalleys.Utilities.Events;

/// <summary>
/// Defines a scoped post-processing event handler for a specific event type.
/// Post-handlers are executed after the main event handlers and are registered as scoped services.
/// </summary>
/// <typeparam name="TEvent">The type of event this handler processes. Must implement <see cref="IEvent"/>.</typeparam>
public interface IEventPostHandler<TEvent> where TEvent : IEvent
{
    /// <summary>
    /// Handles the specified event asynchronously during the post-processing phase.
    /// </summary>
    /// <param name="eventData">The event data to handle.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous post-processing operation.</returns>
    Task HandleAsync(TEvent eventData, CancellationToken cancellationToken = default);
}
