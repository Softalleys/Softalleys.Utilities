using Microsoft.Extensions.Hosting;

namespace Softalleys.Utilities.Events;

/// <summary>
/// Defines an event handler that is also a hosted/background service.
/// A single singleton instance should back both the IHostedService and the event handler interface.
/// </summary>
/// <typeparam name="TEvent">The event type this hosted handler processes.</typeparam>
public interface IEventHostedService<TEvent> : IHostedService where TEvent : IEvent
{
    /// <summary>
    /// Handles the incoming event data. Implementations can choose to enqueue
    /// the data for background processing managed by the hosted service lifecycle.
    /// </summary>
    /// <param name="eventData">The event payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task HandleAsync(TEvent eventData, CancellationToken cancellationToken = default);
}
