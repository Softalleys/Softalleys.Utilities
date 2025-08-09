namespace Softalleys.Utilities.Queries;

/// <summary>
/// Handles a query and returns a response.
/// </summary>
/// <typeparam name="TQuery">The query type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    /// <summary>
    /// Handles the specified query and returns the response.
    /// </summary>
    /// <param name="query">The query to handle.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The response.</returns>
    Task<TResponse> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
