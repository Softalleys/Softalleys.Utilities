namespace Softalleys.Utilities.Commands;

/// <summary>
/// Optional pipeline abstraction for orchestrating validator, processor, and optional side effects.
/// Implemented by the default handler but can be customized per domain if desired.
/// </summary>
/// <typeparam name="TCommand">The command type.</typeparam>
/// <typeparam name="TResult">The result type.</typeparam>
public interface ICommandPipeline<in TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    Task<TResult> ExecuteAsync(TCommand command, CancellationToken cancellationToken = default);
}
