using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace Softalleys.Utilities.Commands;

public sealed class HandlerInvokerCache : IHandlerInvokerCache
{
    private readonly ConcurrentDictionary<(Type cmd, Type res), Func<object, object, CancellationToken, Task<object?>>> _handlerInvokers
        = new();

    private readonly ConcurrentDictionary<(Type cmd, Type res), Func<object?, object?, object, object>> _defaultHandlerFactories
        = new();

    public Func<object, object, CancellationToken, Task<object?>> GetOrAddHandlerInvoker(Type commandType, Type resultType)
    {
        var key = (commandType, resultType);
        return _handlerInvokers.GetOrAdd(key, _ => CreateHandlerInvoker(commandType, resultType));
    }

    public Func<object?, object?, object, object> GetOrAddDefaultHandlerFactory(Type commandType, Type resultType)
    {
        var key = (commandType, resultType);
        return _defaultHandlerFactories.GetOrAdd(key, _ => CreateDefaultHandlerFactory(commandType, resultType));
    }

    private static Func<object, object, CancellationToken, Task<object?>> CreateHandlerInvoker(Type commandType, Type resultType)
    {
        // Build a delegate that calls ((ICommandHandler<TCommand,TResult>)handler).HandleAsync((TCommand)command, ct)
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(commandType, resultType);
        var method = handlerType.GetMethod("HandleAsync", new[] { commandType, typeof(CancellationToken) })
            ?? throw new InvalidOperationException("HandleAsync method not found on handler type.");

        // Build expression: (object handler, object command, CancellationToken ct) => ((ICommandHandler<TCommand,TResult>)handler).HandleAsync((TCommand)command, ct)
        var handlerParam = Expression.Parameter(typeof(object), "handler");
        var commandParam = Expression.Parameter(typeof(object), "command");
        var ctParam = Expression.Parameter(typeof(CancellationToken), "ct");

        var castHandler = Expression.Convert(handlerParam, handlerType);
        var castCommand = Expression.Convert(commandParam, commandType);

        var call = Expression.Call(castHandler, method, castCommand, ctParam);

        // call returns Task<TResult>; convert to object via Task continuation helper
        // We will wrap to Task<object?> by using a helper method at runtime via reflection (InvokeAsyncHelper)
        var helperGeneric = typeof(HandlerInvokerCache).GetMethod(nameof(InvokeAsyncHelper), BindingFlags.NonPublic | BindingFlags.Static)!;
        var helperClosed = helperGeneric.MakeGenericMethod(resultType);

        var lambda = Expression.Lambda<Func<object, object, CancellationToken, Task<object?>>>(
            Expression.Call(helperClosed, call),
            handlerParam, commandParam, ctParam);

        return lambda.Compile();
    }

    private static Func<object?, object?, object, object> CreateDefaultHandlerFactory(Type commandType, Type resultType)
    {
        var handlerGenericType = typeof(DefaultCommandHandler<,>).MakeGenericType(commandType, resultType);

        var validatorType = typeof(ICommandValidator<,>).MakeGenericType(commandType, resultType);
        var processorType = typeof(ICommandProcessor<,>).MakeGenericType(commandType, resultType);
        var postActionsType = typeof(IEnumerable<>).MakeGenericType(typeof(ICommandPostAction<,>).MakeGenericType(commandType, resultType));

        var ctor = handlerGenericType.GetConstructor(new[] { validatorType, processorType, postActionsType })
            ?? throw new InvalidOperationException($"No matching ctor for {handlerGenericType}");

        var vParam = Expression.Parameter(typeof(object), "validatorObj");
        var pParam = Expression.Parameter(typeof(object), "processorObj");
        var paParam = Expression.Parameter(typeof(object), "postActionsObj");

        var validatorCast = Expression.Convert(vParam, validatorType);
        var processorCast = Expression.Convert(pParam, processorType);
        var postActionsCast = Expression.Convert(paParam, postActionsType);

        var newExpr = Expression.New(ctor, validatorCast, processorCast, postActionsCast);
        var lambda = Expression.Lambda<Func<object?, object?, object, object>>(Expression.Convert(newExpr, typeof(object)), vParam, pParam, paParam);
        return lambda.Compile();
    }

    // Helper that turns Task<TResult> into Task<object?> compatible with the delegate signature.
    private static async Task<object?> InvokeAsyncHelper<TResult>(Task<TResult> task)
    {
        var res = await task.ConfigureAwait(false);
        return (object?)res;
    }

}
