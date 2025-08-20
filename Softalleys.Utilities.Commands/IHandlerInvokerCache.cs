using System;
using System.Threading;
using System.Threading.Tasks;

namespace Softalleys.Utilities.Commands;

/// <summary>
/// Provides cached delegates to invoke handlers and to create default handler instances.
/// </summary>
public interface IHandlerInvokerCache
{
    /// <summary>
    /// Returns a delegate that invokes HandleAsync on a handler instance for the given command/result types.
    /// Delegate signature: Func<object handler, object command, CancellationToken ct, Task<object?>>
    /// </summary>
    Func<object, object, CancellationToken, Task<object?>> GetOrAddHandlerInvoker(Type commandType, Type resultType);

    /// <summary>
    /// Returns a factory delegate that constructs DefaultCommandHandler&lt;TCommand,TResult&gt; given (validator, processor, postActions).
    /// Delegate signature: Func<object? validator, object? processor, object postActions, object handlerInstance>
    /// </summary>
    Func<object?, object?, object, object> GetOrAddDefaultHandlerFactory(Type commandType, Type resultType);
}
