namespace Softalleys.Utilities.Commands;

/// <summary>
/// Handles a command by validating and/or executing business logic to produce a result.
/// </summary>
/// <typeparam name="TCommand">The command type.</typeparam>
/// <typeparam name="TResult">The result type.</typeparam>
public interface ICommandHandler<in TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    /// <summary>
    /// Handles the specified command end-to-end (it may itself call validators/processors), returning a result.
    /// </summary>
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}
