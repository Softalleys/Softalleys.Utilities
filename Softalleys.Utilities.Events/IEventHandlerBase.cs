namespace Softalleys.Utilities.Events;

/// <summary>
/// Defines a base interface for handling events asynchronously.
/// </summary>
public interface IEventHandlerBase<T> where T : IEvent
{
    /// <summary>
    /// Handles the specified event asynchronously.
    /// </summary>
    /// <param name="eventData">The event data to handle.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous event handling operation.</returns>
    Task HandleAsync(T eventData, CancellationToken cancellationToken = default);
}
