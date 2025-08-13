namespace Softalleys.Utilities.Events;

/// <summary>
/// Defines a singleton pre-processing event handler for a specific event type.
/// Pre-handlers are executed before the main event handlers and are registered as singleton services.
/// </summary>
/// <typeparam name="TEvent">The type of event this handler processes. Must implement <see cref="IEvent"/>.</typeparam>
public interface IEventPreSingletonHandler<TEvent> : IEventHandlerBase<TEvent> where TEvent : IEvent { }
