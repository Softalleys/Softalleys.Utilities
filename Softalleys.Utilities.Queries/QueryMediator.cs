namespace Softalleys.Utilities.Queries;

/// <summary>
/// Default implementation of the query mediator that delegates to IQueryDispatcher.
/// This provides symmetry with ICommandMediator's SendAsync method naming.
/// </summary>
public class QueryMediator : IQueryMediator
{
    private readonly IQueryDispatcher _dispatcher;

    public QueryMediator(IQueryDispatcher dispatcher)
    {
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
    }

    public async Task<TResponse> SendAsync<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default)
    {
        return await _dispatcher.DispatchAsync(query, cancellationToken).ConfigureAwait(false);
    }

    public IAsyncEnumerable<TResponse> SendStreamAsync<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default)
    {
        return _dispatcher.DispatchStreamAsync(query, cancellationToken);
    }
}