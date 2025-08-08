namespace Softalleys.Utilities.Events;

/// <summary>
/// Defines a scoped pre-processing event handler for a specific event type.
/// Pre-handlers are executed before the main event handlers and are registered as scoped services.
/// </summary>
/// <typeparam name="TEvent">The type of event this handler processes. Must implement <see cref="IEvent"/>.</typeparam>
public interface IEventPreHandler<TEvent> where TEvent : IEvent
{
    /// <summary>
    /// Handles the specified event asynchronously during the pre-processing phase.
    /// </summary>
    /// <param name="eventData">The event data to handle.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous pre-processing operation.</returns>
    Task HandleAsync(TEvent eventData, CancellationToken cancellationToken = default);
}
