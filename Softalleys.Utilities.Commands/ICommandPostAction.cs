namespace Softalleys.Utilities.Commands;

/// <summary>
/// Optional post-action executed after processing succeeds (e.g., publish events, logs, audit).
/// Implementations should be side-effect oriented and idempotent when possible.
/// </summary>
public interface ICommandPostAction<in TCommand, in TResult>
    where TCommand : ICommand<TResult>
{
    Task ExecuteAsync(TCommand command, TResult result, CancellationToken cancellationToken = default);
}
