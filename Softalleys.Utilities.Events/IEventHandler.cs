namespace Softalleys.Utilities.Events;

/// <summary>
/// Defines a scoped event handler for a specific event type.
/// Handlers implementing this interface are registered as scoped services and 
/// are resolved within the current DI scope.
/// </summary>
/// <typeparam name="TEvent">The type of event this handler processes. Must implement <see cref="IEvent"/>.</typeparam>
public interface IEventHandler<TEvent> : IEventHandlerBase<TEvent> where TEvent : IEvent { }
