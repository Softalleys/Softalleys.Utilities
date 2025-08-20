namespace Softalleys.Utilities.Commands;

/// <summary>
/// Mediates sending commands to handlers.
/// </summary>
public interface ICommandMediator
{
    /// <summary>
    /// Sends a strongly-typed command to its registered handler.
    /// </summary>
    /// <typeparam name="TResult">The command result type.</typeparam>
    /// <typeparam name="TCommand">The concrete command type implementing <see cref="ICommand{TResult}"/>.</typeparam>
    /// <param name="command">The command instance.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that yields the command result.</returns>
    /// <remarks>
    /// This overload is the preferred option for performance-sensitive code because the compiler can infer
    /// generic arguments from the concrete <typeparamref name="TCommand"/> type and the implementation
    /// resolves handlers using generic service types (minimal reflection).
    /// </remarks>
    Task<TResult> SendAsync<TResult, TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResult>;

    /// <summary>
    /// Sends a command instance when only the result generic argument is available at compile time.
    /// </summary>
    /// <typeparam name="TResult">The command result type.</typeparam>
    /// <param name="command">The command instance implementing <see cref="ICommand{TResult}"/>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that yields the command result.</returns>
    /// <remarks>
    /// This overload accepts an <see cref="ICommand{TResult}"/> instance and therefore requires the
    /// implementation to discover the concrete command type at runtime (uses limited reflection).
    /// Use the generic overload when possible for slightly better performance; however, this overload
    /// is useful when callers only have the <see cref="ICommand{TResult}"/> interface at hand.
    /// </remarks>
    Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default);
}
