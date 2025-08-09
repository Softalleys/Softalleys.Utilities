namespace Softalleys.Utilities.Queries;

/// <summary>
/// Handles a query as a stream and returns an async sequence of responses.
/// </summary>
/// <typeparam name="TQuery">The query type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public interface IQueryStreamHandler<in TQuery, TResponse>
    where TQuery : IQuery<IAsyncEnumerable<TResponse>>
{
    /// <summary>
    /// Handles the specified query and returns an async sequence of responses.
    /// </summary>
    /// <param name="query">The query to handle.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>An async sequence of responses.</returns>
    IAsyncEnumerable<TResponse> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
