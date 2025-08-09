namespace Softalleys.Utilities.Queries;

/// <summary>
/// Marker interface to indicate a query handler prefers Singleton lifetime.
/// </summary>
/// <typeparam name="TQuery">The query type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public interface IQuerySingletonHandler<in TQuery, TResponse> : IQueryHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
}
