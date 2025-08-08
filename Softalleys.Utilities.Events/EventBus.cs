using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Softalleys.Utilities.Events;

/// <summary>
/// Default implementation of <see cref="IEventBus"/> that manages event handler execution
/// with proper dependency injection scope handling and ordered execution pipeline.
/// </summary>
public class EventBus : IEventBus
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventBus> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventBus"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider for resolving handlers.</param>
    /// <param name="logger">The logger for event bus operations.</param>
    public EventBus(IServiceProvider serviceProvider, ILogger<EventBus>? logger = null)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<EventBus>.Instance;
    }

    /// <inheritdoc />
    public async Task PublishAsync<TEvent>(TEvent eventData, CancellationToken cancellationToken = default) where TEvent : IEvent
    {
        if (eventData == null)
            throw new ArgumentNullException(nameof(eventData));

        var eventType = typeof(TEvent);
        _logger.LogDebug("Publishing event {EventType}", eventType.Name);

        var exceptions = new List<Exception>();

        try
        {
            // Phase 1: Pre-processing handlers (Singleton first, then Scoped)
            await ExecuteHandlersAsync<IEventPreSingletonHandler<TEvent>>(eventData, cancellationToken, exceptions, "Pre-Singleton");
            await ExecuteHandlersAsync<IEventPreHandler<TEvent>>(eventData, cancellationToken, exceptions, "Pre-Scoped");

            // Phase 2: Main handlers (Singleton first, then Scoped)
            await ExecuteHandlersAsync<IEventSingletonHandler<TEvent>>(eventData, cancellationToken, exceptions, "Main-Singleton");
            await ExecuteHandlersAsync<IEventHandler<TEvent>>(eventData, cancellationToken, exceptions, "Main-Scoped");

            // Phase 3: Post-processing handlers (Singleton first, then Scoped)
            await ExecuteHandlersAsync<IEventPostSingletonHandler<TEvent>>(eventData, cancellationToken, exceptions, "Post-Singleton");
            await ExecuteHandlersAsync<IEventPostHandler<TEvent>>(eventData, cancellationToken, exceptions, "Post-Scoped");

            _logger.LogDebug("Successfully published event {EventType}", eventType.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while publishing event {EventType}", eventType.Name);
            exceptions.Add(ex);
        }

        // Throw aggregate exception if any handlers failed
        if (exceptions.Count > 0)
        {
            throw new AggregateException($"One or more handlers failed while processing event {eventType.Name}", exceptions);
        }
    }

    /// <summary>
    /// Executes all handlers of the specified type for the given event.
    /// </summary>
    /// <typeparam name="THandler">The handler interface type.</typeparam>
    /// <param name="eventData">The event data to pass to handlers.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="exceptions">List to collect any exceptions that occur.</param>
    /// <param name="phase">The execution phase name for logging.</param>
    private async Task ExecuteHandlersAsync<THandler>(IEvent eventData, CancellationToken cancellationToken, List<Exception> exceptions, string phase)
        where THandler : class
    {
        var handlers = _serviceProvider.GetServices<THandler>();
        var handlersList = handlers.ToList();

        if (!handlersList.Any())
        {
            _logger.LogTrace("No {Phase} handlers found for event {EventType}", phase, eventData.GetType().Name);
            return;
        }

        _logger.LogTrace("Executing {Count} {Phase} handlers for event {EventType}", handlersList.Count, phase, eventData.GetType().Name);

        var tasks = handlersList.Select(async handler =>
        {
            try
            {
                // Use reflection to call HandleAsync method
                var handleMethod = handler.GetType().GetMethod("HandleAsync");
                if (handleMethod != null)
                {
                    var task = handleMethod.Invoke(handler, new object[] { eventData, cancellationToken }) as Task;
                    if (task != null)
                    {
                        await task.ConfigureAwait(false);
                        _logger.LogTrace("Successfully executed {Phase} handler {HandlerType}", phase, handler.GetType().Name);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Handler {HandlerType} failed during {Phase} phase for event {EventType}", 
                    handler.GetType().Name, phase, eventData.GetType().Name);
                
                // Collect exception but don't stop other handlers
                lock (exceptions)
                {
                    exceptions.Add(new InvalidOperationException(
                        $"Handler {handler.GetType().Name} failed during {phase} phase", ex));
                }
            }
        });

        // Execute all handlers concurrently within the same phase
        await Task.WhenAll(tasks).ConfigureAwait(false);
    }
}
