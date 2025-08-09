namespace Softalleys.Utilities.Queries;

/// <summary>
/// Marker interface to indicate a query stream handler prefers Singleton lifetime.
/// </summary>
/// <typeparam name="TQuery">The query type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public interface IQueryStreamSingletonHandler<in TQuery, TResponse> : IQueryStreamHandler<TQuery, TResponse>
    where TQuery : IQuery<IAsyncEnumerable<TResponse>>
{
}
