namespace Softalleys.Utilities.Queries;

/// <summary>
/// Handles a query as a stream and returns an async sequence of responses.
/// The query type is the same used for single-result handlers: <see cref="IQuery{TResponse}"/>.
/// </summary>
/// <typeparam name="TQuery">The query type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public interface IQueryStreamHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    /// <summary>
    /// Streams the results for the specified query as an async sequence.
    /// </summary>
    /// <param name="query">The query to stream.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>An async sequence of responses.</returns>
    IAsyncEnumerable<TResponse> StreamAsync(TQuery query, CancellationToken cancellationToken = default);
}
