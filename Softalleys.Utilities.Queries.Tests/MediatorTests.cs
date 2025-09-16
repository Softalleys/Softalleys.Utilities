using Microsoft.Extensions.DependencyInjection;

namespace Softalleys.Utilities.Queries.Tests;

public class MediatorTests
{
    private class Ping : IQuery<string> { public string Message { get; init; } = ""; }

    private class PingHandler : IQueryHandler<Ping, string>
    {
        public Task<string> HandleAsync(Ping query, CancellationToken cancellationToken = default)
            => Task.FromResult($"pong:{query.Message}");
    }

    private class Counter : IQuery<int> { public int Count { get; init; } }

    private class CounterStreamHandler : IQueryStreamHandler<Counter, int>
    {
        public async IAsyncEnumerable<int> StreamAsync(Counter query, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            for (var i = 0; i < query.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return i;
                await Task.Yield();
            }
        }
    }

    private class BothEcho : IQuery<string> { public string Text { get; init; } = string.Empty; }
    private class BothEchoHandler : IQueryHandler<BothEcho, string>, IQueryStreamHandler<BothEcho, string>
    {
        public Task<string> HandleAsync(BothEcho query, CancellationToken cancellationToken = default)
            => Task.FromResult(query.Text);

        public async IAsyncEnumerable<string> StreamAsync(BothEcho query, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (var ch in query.Text)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return ch.ToString();
                await Task.Yield();
            }
        }
    }

    [Fact]
    public async Task Mediator_SendAsync_Should_Work_Like_Dispatcher_DispatchAsync()
    {
        var services = new ServiceCollection()
            .AddSoftalleysQueries(typeof(PingHandler).Assembly)
            .BuildServiceProvider();

        var dispatcher = services.GetRequiredService<IQueryDispatcher>();
        var mediator = services.GetRequiredService<IQueryMediator>();

        var query = new Ping { Message = "test" };
        
        var dispatcherResult = await dispatcher.DispatchAsync(query);
        var mediatorResult = await mediator.SendAsync(query);

        Assert.Equal(dispatcherResult, mediatorResult);
        Assert.Equal("pong:test", mediatorResult);
    }

    [Fact]
    public async Task Mediator_SendStreamAsync_Should_Work_Like_Dispatcher_DispatchStreamAsync()
    {
        var services = new ServiceCollection()
            .AddSoftalleysQueries(typeof(CounterStreamHandler).Assembly)
            .BuildServiceProvider();

        var dispatcher = services.GetRequiredService<IQueryDispatcher>();
        var mediator = services.GetRequiredService<IQueryMediator>();

        var query = new Counter { Count = 3 };
        
        var dispatcherResults = new List<int>();
        await foreach (var result in dispatcher.DispatchStreamAsync<int>(query))
        {
            dispatcherResults.Add(result);
        }

        var mediatorResults = new List<int>();
        await foreach (var result in mediator.SendStreamAsync<int>(query))
        {
            mediatorResults.Add(result);
        }

        Assert.Equal(dispatcherResults, mediatorResults);
        Assert.Equal(new[] { 0, 1, 2 }, mediatorResults);
    }

    [Fact]
    public async Task Mediator_Should_Handle_Both_Single_And_Stream_Like_Dispatcher()
    {
        var services = new ServiceCollection()
            .AddSoftalleysQueries(typeof(BothEchoHandler).Assembly)
            .BuildServiceProvider();

        var dispatcher = services.GetRequiredService<IQueryDispatcher>();
        var mediator = services.GetRequiredService<IQueryMediator>();

        var query = new BothEcho { Text = "xy" };

        // Test single result
        var dispatcherSingle = await dispatcher.DispatchAsync(query);
        var mediatorSingle = await mediator.SendAsync(query);
        Assert.Equal(dispatcherSingle, mediatorSingle);
        Assert.Equal("xy", mediatorSingle);

        // Test streamed result
        var dispatcherStream = new List<string>();
        await foreach (var s in dispatcher.DispatchStreamAsync<string>(query))
        {
            dispatcherStream.Add(s);
        }

        var mediatorStream = new List<string>();
        await foreach (var s in mediator.SendStreamAsync<string>(query))
        {
            mediatorStream.Add(s);
        }

        Assert.Equal(dispatcherStream, mediatorStream);
        Assert.Equal(new[] { "x", "y" }, mediatorStream);
    }

    [Fact]
    public void Mediator_Should_Be_Registered_As_Singleton()
    {
        var services = new ServiceCollection()
            .AddSoftalleysQueries(typeof(PingHandler).Assembly)
            .BuildServiceProvider();

        var mediator1 = services.GetRequiredService<IQueryMediator>();
        var mediator2 = services.GetRequiredService<IQueryMediator>();

        Assert.Same(mediator1, mediator2);
    }

    [Fact]
    public async Task Mediator_Should_Throw_When_No_Handler_Registered()
    {
        // Create a query type that has no handler
        var noHandlerQuery = new NoHandlerQuery();
        
        var services = new ServiceCollection()
            .AddSoftalleysQueries(typeof(PingHandler).Assembly)
            .BuildServiceProvider();

        var mediator = services.GetRequiredService<IQueryMediator>();

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await mediator.SendAsync(noHandlerQuery);
        });
    }

    private class NoHandlerQuery : IQuery<string> { }
}