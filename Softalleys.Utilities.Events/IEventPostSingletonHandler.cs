namespace Softalleys.Utilities.Events;

/// <summary>
/// Defines a singleton post-processing event handler for a specific event type.
/// Post-handlers are executed after the main event handlers and are registered as singleton services.
/// </summary>
/// <typeparam name="TEvent">The type of event this handler processes. Must implement <see cref="IEvent"/>.</typeparam>
public interface IEventPostSingletonHandler<TEvent> : IEventHandlerBase<TEvent> where TEvent : IEvent { }
