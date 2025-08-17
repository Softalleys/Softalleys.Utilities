namespace Softalleys.Utilities.Commands;

/// <summary>
/// Mediates sending commands to handlers.
/// </summary>
public interface ICommandMediator
{
    Task<TResult> SendAsync<TResult, TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResult>;
}
