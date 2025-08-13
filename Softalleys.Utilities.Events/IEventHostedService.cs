using Microsoft.Extensions.Hosting;

namespace Softalleys.Utilities.Events;

/// <summary>
/// Defines an event handler that is also a hosted/background service.
/// A single singleton instance should back both the IHostedService and the event handler interface.
/// </summary>
/// <typeparam name="TEvent">The event type this hosted handler processes.</typeparam>
public interface IEventHostedService<TEvent> : IEventHandlerBase<TEvent> where TEvent : IEvent { }
