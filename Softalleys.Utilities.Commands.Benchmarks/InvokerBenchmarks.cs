using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace Softalleys.Utilities.Commands.Benchmarks;

[MemoryDiagnoser]
[SimpleJob]
public class InvokerBenchmarks
{
    private IServiceProvider _sp = default!;
    private ICommandMediator _mediator = default!;
    private IHandlerInvokerCache _cache = default!;

    private object _handler = default!;
    private object _defaultHandler = default!;
    private object _command = default!;
    private Type _cmdType = default!;
    private Type _resType = default!;

    private Delegate _reflectHandle = default!; // cached reflection wrapper

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection().AddSoftalleysCommands(typeof(AddHandler).Assembly);
        _sp = services.BuildServiceProvider();
        _mediator = _sp.GetRequiredService<ICommandMediator>();
        _cache = _sp.GetRequiredService<IHandlerInvokerCache>();

        _cmdType = typeof(AddCommand);
        _resType = typeof(ResultBase);
        _command = new AddCommand(10, 20);

        // resolve real handler (none registered), so build default one via cache
        var validator = Activator.CreateInstance(typeof(AddValidator));
        var processor = Activator.CreateInstance(typeof(AddProcessor));
        var postActions = Array.Empty<ICommandPostAction<AddCommand, ResultBase>>();
        var factory = _cache.GetOrAddDefaultHandlerFactory(_cmdType, _resType);
        _defaultHandler = factory(validator!, processor!, postActions);

        // reflection path: build MethodInfo once and wrap to a delegate to fairly compare per-invoke cost
        var handlerType = typeof(DefaultCommandHandler<,>).MakeGenericType(_cmdType, _resType);
        var method = handlerType.GetMethod("HandleAsync", new[] { _cmdType, typeof(CancellationToken) })!;
        _reflectHandle = BuildReflectionWrapper(method);

        _handler = _defaultHandler;
    }

    private static Func<object, object, CancellationToken, Task<object?>> BuildReflectionWrapper(System.Reflection.MethodInfo method)
    {
        return async (handler, command, ct) =>
        {
            var taskObj = method.Invoke(handler, new object?[] { command, ct });
            if (taskObj is Task t)
            {
                await t.ConfigureAwait(false);
                var resultProp = t.GetType().GetProperty("Result") ?? throw new InvalidOperationException("Expected Task<TResult>");
                return (object?)resultProp.GetValue(t);
            }
            throw new InvalidOperationException("Expected Task");
        };
    }

    [Benchmark(Baseline = true)]
    public async Task<object?> Reflection_Invoke_DefaultHandler()
    {
        var inv = (Func<object, object, CancellationToken, Task<object?>>)_reflectHandle;
        return await inv(_handler, _command, CancellationToken.None);
    }

    [Benchmark]
    public async Task<object?> CachedDelegate_Invoke_DefaultHandler()
    {
        var inv = _cache.GetOrAddHandlerInvoker(_cmdType, _resType);
        return await inv(_handler, _command, CancellationToken.None);
    }

    // Domain types copied from tests
    private abstract record ResultBase;
    private record Valid : ResultBase, IValidationStageResult { public bool Continue => true; }
    private record Success(int Value) : ResultBase;
    private record Failure(string Title, string Message) : ResultBase
    {
        // simplified for benchmark only
    }

    private record AddCommand(int A, int B) : ICommand<ResultBase>;

    private class AddValidator : ICommandValidator<AddCommand, ResultBase>
    {
        public Task<ResultBase> ValidateAsync(AddCommand command, CancellationToken cancellationToken = default)
            => command.A < 0 || command.B < 0
                ? Task.FromResult<ResultBase>(new Failure("Invalid", "Negative numbers not allowed"))
                : Task.FromResult<ResultBase>(new Valid());
    }

    private class AddProcessor : ICommandProcessor<AddCommand, ResultBase>
    {
        public Task<ResultBase> ProcessAsync(AddCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult<ResultBase>(new Success(command.A + command.B));
    }

    private class AddHandler : DefaultCommandHandler<AddCommand, ResultBase>
    {
        public AddHandler(ICommandValidator<AddCommand, ResultBase> v, ICommandProcessor<AddCommand, ResultBase> p)
            : base(v, p) { }
    }
}
