namespace Softalleys.Utilities.Interfaces;

/// <summary>
/// Defines a contract for handling requests asynchronously, producing a corresponding result.
/// </summary>
/// <typeparam name="TRequest">
/// The type representing the input request. It must be a reference type.
/// </typeparam>
/// <typeparam name="TResult">
/// The type representing the output result. It must be a reference type.
/// </typeparam>
public interface IHandler<in TRequest, TResult>
    where TRequest : class
    where TResult : class
{
    /// <summary>
    /// Asynchronously processes the provided request and returns a result.
    /// </summary>
    /// <param name="request">
    /// The request to handle.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation, containing the result of the handling process.
    /// </returns>
    Task<TResult> HandleAsync(TRequest request);
}