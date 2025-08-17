namespace Softalleys.Utilities.Commands;

/// <summary>
/// Marker interface to request Singleton lifetime for a command handler.
/// </summary>
/// <typeparam name="TCommand">The command type.</typeparam>
/// <typeparam name="TResult">The result type.</typeparam>
public interface ICommandSingletonHandler<in TCommand, TResult> : ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
}
