using Microsoft.Extensions.DependencyInjection;

namespace Softalleys.Utilities.Commands;

/// <summary>
/// Default mediator implementation that creates a scope for each command and resolves the appropriate handler.
/// </summary>
public class CommandMediator(IServiceProvider sp, IHandlerInvokerCache invokerCache) : ICommandMediator
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
        var processors = sp.GetServices<ICommandProcessor<TCommand, TResult>>().ToList();
        if (processors.Count == 0)
        {
            throw new InvalidOperationException(
                $"No ICommandHandler<{typeof(TCommand).Name}, {typeof(TResult).Name}> or ICommandProcessor<{typeof(TCommand).Name}, {typeof(TResult).Name}> is registered.");
        }

        var validators = sp.GetServices<ICommandValidator<TCommand, TResult>>();
        var postActions = sp.GetServices<ICommandPostAction<TCommand, TResult>>();

        var defaultHandler = new DefaultCommandHandler<TCommand, TResult>(validators, processors, postActions);
        return await defaultHandler.HandleAsync(command, cancellationToken).ConfigureAwait(false);
    }

    public async Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        // Mediator is registered as Scoped; use the scoped provider directly

        var commandType = command.GetType();

        // 1. Try to resolve a full handler first
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(commandType, typeof(TResult));
        var handlers = sp.GetServices(handlerType).Cast<object>().ToArray();
        if (handlers.Length > 1)
        {
            throw new InvalidOperationException($"Multiple ICommandHandler<{commandType.Name}, {typeof(TResult).Name}> registrations found.");
        }
        var handler = handlers.FirstOrDefault();
        if (handler is not null)
        {
            var invoker = invokerCache.GetOrAddHandlerInvoker(commandType, typeof(TResult));
            var result = await invoker(handler, command, cancellationToken).ConfigureAwait(false);
            return (TResult)result!;
        }

        // 2. Fallback: compose DefaultCommandHandler from parts
        var processorType = typeof(ICommandProcessor<,>).MakeGenericType(commandType, typeof(TResult));
        var processors = sp.GetServices(processorType).Cast<object>().ToArray();
        if (processors.Length == 0)
        {
            throw new InvalidOperationException($"No ICommandHandler<{commandType.Name}, {typeof(TResult).Name}> or ICommandProcessor<{commandType.Name}, {typeof(TResult).Name}> is registered.");
        }

        var validatorType = typeof(ICommandValidator<,>).MakeGenericType(commandType, typeof(TResult));
        var validators = sp.GetServices(validatorType);
        var postActionType = typeof(ICommandPostAction<,>).MakeGenericType(commandType, typeof(TResult));
        var postActions = sp.GetServices(postActionType);

    // Create DefaultCommandHandler<TCommand, TResult> via compiled factory
    var factory = invokerCache.GetOrAddDefaultHandlerFactory(commandType, typeof(TResult));
    var defaultHandler = factory(validators, processors, postActions);

    // Invoke HandleAsync on the constructed default handler via compiled invoker
    var defInvoker = invokerCache.GetOrAddHandlerInvoker(commandType, typeof(TResult));
    var defResult = await defInvoker(defaultHandler, command, cancellationToken).ConfigureAwait(false);
    return (TResult)defResult!;
    }
}