namespace Softalleys.Utilities.Queries;

/// <summary>
/// Marker interface for a query that returns a response of type <typeparamref name="TResponse"/>.
/// </summary>
/// <typeparam name="TResponse">The type of the response produced by the query.</typeparam>
public interface IQuery<out TResponse>
{
}
