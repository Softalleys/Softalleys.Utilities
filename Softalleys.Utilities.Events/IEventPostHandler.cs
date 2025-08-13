namespace Softalleys.Utilities.Events;

/// <summary>
/// Defines a scoped post-processing event handler for a specific event type.
/// Post-handlers are executed after the main event handlers and are registered as scoped services.
/// </summary>
/// <typeparam name="TEvent">The type of event this handler processes. Must implement <see cref="IEvent"/>.</typeparam>
public interface IEventPostHandler<TEvent> : IEventHandlerBase<TEvent> where TEvent : IEvent { }
