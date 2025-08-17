namespace Softalleys.Utilities.Commands;

/// <summary>
/// Validates a command and returns a result indicating either Valid or Failure (implementations/classes are domain-specific).
/// </summary>
/// <typeparam name="TCommand">The command type.</typeparam>
/// <typeparam name="TResult">The result type returned by the pipeline.</typeparam>
public interface ICommandValidator<in TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    Task<TResult> ValidateAsync(TCommand command, CancellationToken cancellationToken = default);
}
