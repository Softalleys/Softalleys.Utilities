using Microsoft.Extensions.DependencyInjection;

namespace Softalleys.Utilities.Commands;

/// <summary>
/// Default mediator implementation that creates a scope for each command and resolves the appropriate handler.
/// </summary>
public class CommandMediator(IServiceProvider sp) : ICommandMediator
{
    public async Task<TResult> SendAsync<TResult, TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResult>
    {
        ArgumentNullException.ThrowIfNull(command);

        // Mediator is registered as Scoped; use the scoped provider directly

        // Try resolve a full handler first
        // Detect multiple handlers and throw to avoid ambiguity
        var handlers = sp.GetServices<ICommandHandler<TCommand, TResult>>().ToArray();
        if (handlers.Length > 1)
        {
            throw new InvalidOperationException($"Multiple ICommandHandler<{typeof(TCommand).Name}, {typeof(TResult).Name}> registrations found.");
        }
        var handler = handlers.FirstOrDefault();
        if (handler is not null)
        {
            return await handler.HandleAsync(command, cancellationToken).ConfigureAwait(false);
        }

        // Fallback: compose DefaultCommandHandler from parts
        var processor = sp.GetService<ICommandProcessor<TCommand, TResult>>();
        if (processor is null)
        {
            throw new InvalidOperationException(
                $"No ICommandHandler<{typeof(TCommand).Name}, {typeof(TResult).Name}> or ICommandProcessor<{typeof(TCommand).Name}, {typeof(TResult).Name}> is registered.");
        }

        var validator = sp.GetService<ICommandValidator<TCommand, TResult>>();
        var postActions = sp.GetService<IEnumerable<ICommandPostAction<TCommand, TResult>>>() ?? [];

        var defaultHandler = new DefaultCommandHandler<TCommand, TResult>(validator, processor, postActions);
        return await defaultHandler.HandleAsync(command, cancellationToken).ConfigureAwait(false);
    }
}
