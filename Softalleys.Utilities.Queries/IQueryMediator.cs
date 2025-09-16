namespace Softalleys.Utilities.Queries;

/// <summary>
/// Mediates sending queries to handlers. This is an alias for IQueryDispatcher that provides 
/// symmetry with ICommandMediator's SendAsync method naming.
/// </summary>
public interface IQueryMediator
{
    /// <summary>
    /// Sends a query to its registered handler and returns the result asynchronously.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response returned by the query.</typeparam>
    /// <param name="query">The query instance to send.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, with the query result.</returns>
    Task<TResponse> SendAsync<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a query to a streaming handler and returns an async sequence of results.
    /// </summary>
    /// <typeparam name="TResponse">The type of the elements in the result stream.</typeparam>
    /// <param name="query">The query instance to send.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>An <see cref="IAsyncEnumerable{TResponse}"/> representing the result stream.</returns>
    IAsyncEnumerable<TResponse> SendStreamAsync<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default);
}