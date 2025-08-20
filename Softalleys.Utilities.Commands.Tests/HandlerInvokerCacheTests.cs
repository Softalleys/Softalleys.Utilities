using Microsoft.Extensions.DependencyInjection;

namespace Softalleys.Utilities.Commands.Tests;

public class HandlerInvokerCacheTests
{
    private record SumCmd(int A, int B) : ICommand<int>;

    private class SumHandler : ICommandHandler<SumCmd, int>
    {
        public Task<int> HandleAsync(SumCmd command, CancellationToken cancellationToken = default)
            => Task.FromResult(command.A + command.B);
    }

    [Fact]
    public async Task Invoker_Executes_Handler_Successfully()
    {
        var cache = new HandlerInvokerCache();
        var invoker = cache.GetOrAddHandlerInvoker(typeof(SumCmd), typeof(int));

        var handler = new SumHandler();
        var result = await invoker(handler, new SumCmd(2, 3), CancellationToken.None);
        Assert.Equal(5, Assert.IsType<int>(result));
    }

    private abstract record ResultBase;
    private record Valid : ResultBase, IValidationStageResult { public bool Continue => true; }
    private record Failure(string Title) : ResultBase;
    private record Payload(int Value) : ResultBase;
    private record PipeCmd(int X) : ICommand<ResultBase>;

    private class PipeValidator : ICommandValidator<PipeCmd, ResultBase>
    {
        public Task<ResultBase> ValidateAsync(PipeCmd command, CancellationToken cancellationToken = default)
            => command.X < 0
                ? Task.FromResult<ResultBase>(new Failure("Invalid"))
                : Task.FromResult<ResultBase>(new Valid());
    }

    private class PipeProcessor : ICommandProcessor<PipeCmd, ResultBase>
    {
        public Task<ResultBase> ProcessAsync(PipeCmd command, CancellationToken cancellationToken = default)
            => Task.FromResult<ResultBase>(new Payload(command.X * 2));
    }

    [Fact]
    public async Task Default_Handler_Factory_Creates_And_Invokes()
    {
        var cache = new HandlerInvokerCache();

        var factory = cache.GetOrAddDefaultHandlerFactory(typeof(PipeCmd), typeof(ResultBase));
        var handlerObj = factory(new PipeValidator(), new PipeProcessor(), Array.Empty<ICommandPostAction<PipeCmd, ResultBase>>());

        var invoker = cache.GetOrAddHandlerInvoker(typeof(PipeCmd), typeof(ResultBase));
        var ok = await invoker(handlerObj, new PipeCmd(7), CancellationToken.None);
        var payload = Assert.IsType<Payload>(ok);
        Assert.Equal(14, payload.Value);

        var fail = await invoker(handlerObj, new PipeCmd(-1), CancellationToken.None);
        Assert.IsType<Failure>(fail);
    }

    [Fact]
    public void Caching_Returns_Same_Delegate_Instance()
    {
        var cache = new HandlerInvokerCache();
        var inv1 = cache.GetOrAddHandlerInvoker(typeof(SumCmd), typeof(int));
        var inv2 = cache.GetOrAddHandlerInvoker(typeof(SumCmd), typeof(int));
        Assert.Same(inv1, inv2);

        var fac1 = cache.GetOrAddDefaultHandlerFactory(typeof(PipeCmd), typeof(ResultBase));
        var fac2 = cache.GetOrAddDefaultHandlerFactory(typeof(PipeCmd), typeof(ResultBase));
        Assert.Same(fac1, fac2);
    }

    private record OtherCmd(int Y) : ICommand<int>;

    [Fact]
    public void Invoker_With_Wrong_HandlerType_Throws_Cast()
    {
        var cache = new HandlerInvokerCache();
        var invoker = cache.GetOrAddHandlerInvoker(typeof(SumCmd), typeof(int));

        var wrongHandler = new object();
        Assert.Throws<InvalidCastException>(() =>
        {
            // throws synchronously due to invalid cast in compiled lambda
            var _ = invoker(wrongHandler, new SumCmd(1, 1), CancellationToken.None);
        });
    }
}
