# Nuget Package for Events Softalleys.Utilities.Events

I want to implement a In House Made Event Driven Pattern on my projects, similar to MediatR and LiteBus done with IEvent, INotification, IEventHandler, and INotificationHandler. The problem with MediatR is that is no longer free and requires an license, and the problem with LiteBus is that the IEventHandlers runs as Transient, so i wont work with the scope, that is something I cant work with so I need you tu do it from scratch.

- Create the interface IEvent so the events classes can implement it.
- Create the interface IEventHandler<TEvent> so the event handlers can implement it. TEvent is the type of event the handler handles and must be implement IEvent
- Create the interface IEventSingletonHandler<TEvent> so the event handlers can implement it as a singleton. TEvent is the type of event the handler handles and must be implement IEvent
- Create the interface IEventPreHandler<TEvent> and IEventPostHandler<TEvent> so the event handlers can implement it for pre-processing (run before the normal handlers) and for post-processing (run after the normal handlers)
- Create the interface IEventPreSingletonHandler<TEvent> and IEventPostSingletonHandler<TEvent> similar to IEventPreHandler and IEventPostHandler but running as a singleton.
- Create the interface IEventBus so the events can be published and subscribed.
- Create the implementation for the IEventBus interface. This implementation should use a DI container to manage the lifecycle of the event handlers and ensure that they are resolved with the correct scope. The IEventHandler should be resolved as a scoped service, while the IEventSingletonHandler should be resolved as a singleton service.
- Create the dependency extension for configure the SoftalleysEvents so it can be easily registered in the DI container and pass one or more Assemblies to scan for event handlers and events automatically. 

Finally Add a README.md file with usage examples and documentation for using the Softalleys.Utilities.Events package.

This package are ment to provide the tools for my organization to follow the Event Driven Architecture principles and implement a robust event handling system with minimal dependencies and maximum flexibility.