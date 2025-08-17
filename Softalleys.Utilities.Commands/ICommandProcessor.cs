namespace Softalleys.Utilities.Commands;

/// <summary>
/// Executes the core business logic for a command and produces a result, assuming validation has passed.
/// </summary>
/// <typeparam name="TCommand">The command type.</typeparam>
/// <typeparam name="TResult">The result type.</typeparam>
public interface ICommandProcessor<in TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    Task<TResult> ProcessAsync(TCommand command, CancellationToken cancellationToken = default);
}
