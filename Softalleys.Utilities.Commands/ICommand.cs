namespace Softalleys.Utilities.Commands;

/// <summary>
/// Marker interface for a command that produces a result of type <typeparamref name="TResult"/>.
/// </summary>
/// <typeparam name="TResult">The command result type. Usually a domain-specific discriminated union (record/class hierarchy).</typeparam>
public interface ICommand<out TResult>
{
}
