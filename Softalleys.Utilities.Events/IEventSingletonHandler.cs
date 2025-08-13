namespace Softalleys.Utilities.Events;

/// <summary>
/// Defines a singleton event handler for a specific event type.
/// Handlers implementing this interface are registered as singleton services and 
/// are resolved once per application lifetime.
/// </summary>
/// <typeparam name="TEvent">The type of event this handler processes. Must implement <see cref="IEvent"/>.</typeparam>
public interface IEventSingletonHandler<TEvent> : IEventHandlerBase<TEvent> where TEvent : IEvent { }