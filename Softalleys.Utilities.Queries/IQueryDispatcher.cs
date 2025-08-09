namespace Softalleys.Utilities.Queries;

/// <summary>
/// Dispatches queries to their appropriate handlers.
/// </summary>
public interface IQueryDispatcher
{
    /// <summary>
    /// Dispatches a query to its registered handler and returns the result asynchronously.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response returned by the query.</typeparam>
    /// <param name="query">The query instance to dispatch.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, with the query result.</returns>
    Task<TResponse> DispatchAsync<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dispatches a streaming query to its registered handler and returns an async sequence of results.
    /// </summary>
    /// <typeparam name="TResponse">The type of the elements in the result stream.</typeparam>
    /// <param name="query">The query instance to dispatch.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>An <see cref="IAsyncEnumerable{TResponse}"/> representing the result stream.</returns>
    IAsyncEnumerable<TResponse> DispatchStreamAsync<TResponse>(IQuery<IAsyncEnumerable<TResponse>> query, CancellationToken cancellationToken = default);
}
