using Microsoft.Extensions.DependencyInjection;

namespace Softalleys.Utilities.Queries;

/// <summary>
/// Default implementation of the query dispatcher.
/// </summary>
public class QueryDispatcher : IQueryDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public QueryDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public async Task<TResponse> DispatchAsync<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default)
    {
        if (query is null) throw new ArgumentNullException(nameof(query));

        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResponse));
        var handlers = _serviceProvider.GetServices(handlerType).Cast<object>().ToList();
        if (handlers.Count == 0)
            throw new InvalidOperationException($"No handler registered for {query.GetType().Name} -> {typeof(TResponse).Name}");
        if (handlers.Count > 1)
            throw new InvalidOperationException($"Multiple handlers registered for {query.GetType().Name} -> {typeof(TResponse).Name}");
        var handler = handlers[0];

        var method = handlerType.GetMethod("HandleAsync")!;
        var task = (Task<TResponse>)method.Invoke(handler, new object[] { query, cancellationToken })!;
        return await task.ConfigureAwait(false);
    }

    public IAsyncEnumerable<TResponse> DispatchStreamAsync<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default)
    {
        if (query is null) throw new ArgumentNullException(nameof(query));

        // Create a scope that will live for the duration of the consumer's enumeration
        var handlerType = typeof(IQueryStreamHandler<,>).MakeGenericType(query.GetType(), typeof(TResponse));
        var streamHandlers = _serviceProvider.GetServices(handlerType).Cast<object>().ToList();
        if (streamHandlers.Count == 0)
        {
            throw new InvalidOperationException($"No stream handler registered for {query.GetType().Name} -> IAsyncEnumerable<{typeof(TResponse).Name}>");
        }
        if (streamHandlers.Count > 1)
        {
            throw new InvalidOperationException($"Multiple stream handlers registered for {query.GetType().Name} -> IAsyncEnumerable<{typeof(TResponse).Name}>");
        }

        var method = handlerType.GetMethod("StreamAsync")!;
        var sequence = (IAsyncEnumerable<TResponse>)method.Invoke(streamHandlers[0], new object[] { query, cancellationToken })!;

        return sequence;
    }
}
