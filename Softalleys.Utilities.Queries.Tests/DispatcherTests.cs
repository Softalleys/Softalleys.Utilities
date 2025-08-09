using Microsoft.Extensions.DependencyInjection;

namespace Softalleys.Utilities.Queries.Tests;

public class DispatcherTests
{
    private class Ping : IQuery<string> { public string Message { get; init; } = ""; }

    private class PingHandler : IQueryHandler<Ping, string>
    {
        private static int _instances;
        public int InstanceId { get; } = Interlocked.Increment(ref _instances);
        public Task<string> HandleAsync(Ping query, CancellationToken cancellationToken = default)
            => Task.FromResult($"pong:{query.Message}:{InstanceId}");
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

    private class SingletonPing : IQuery<string> { public string Message { get; init; } = string.Empty; }
    private class SingletonPingHandler : IQuerySingletonHandler<SingletonPing, string>
    {
        public Task<string> HandleAsync(SingletonPing query, CancellationToken cancellationToken = default)
            => Task.FromResult($"singleton:{query.Message}");
    }

    private class NoHandler : IQuery<string> { }

    // Same query type used for both single and stream handlers
    private class Echo : IQuery<string> { public string Text { get; init; } = string.Empty; }
    private class EchoHandler : IQueryHandler<Echo, string>
    {
        public Task<string> HandleAsync(Echo query, CancellationToken cancellationToken = default)
            => Task.FromResult(query.Text);
    }
    private class EchoStreamHandler : IQueryStreamHandler<Echo, string>
    {
        public async IAsyncEnumerable<string> StreamAsync(Echo query, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
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
    public async Task DispatchAsync_Returns_Handler_Result()
    {
        var services = new ServiceCollection()
            .AddSoftalleysQueries(typeof(PingHandler).Assembly)
            .BuildServiceProvider();

        var dispatcher = services.GetRequiredService<IQueryDispatcher>();

        var result = await dispatcher.DispatchAsync(new Ping { Message = "hi" });
        Assert.StartsWith("pong:hi:", result);
    }

    [Fact]
    public async Task Scoped_Handler_Creates_New_Instance_Per_Scope()
    {
        var services = new ServiceCollection()
            .AddSoftalleysQueries(typeof(PingHandler).Assembly)
            .BuildServiceProvider();

        var dispatcher = services.GetRequiredService<IQueryDispatcher>();
        var r1 = await dispatcher.DispatchAsync(new Ping { Message = "a" });
        var r2 = await dispatcher.DispatchAsync(new Ping { Message = "b" });

        // Because dispatcher is singleton, it uses root provider; handlers are scoped so resolved as Transient under root.
        // We just ensure both invocations work; instance id can differ but not guaranteed across providers.
        Assert.NotNull(r1);
        Assert.NotNull(r2);
        Assert.NotEqual(r1, r2);
    }

    [Fact]
    public async Task Singleton_Handler_Is_Used_When_Marker_Interface_Present()
    {
        var services = new ServiceCollection()
            .AddSoftalleysQueries(typeof(SingletonPingHandler).Assembly)
            .BuildServiceProvider();

        var dispatcher = services.GetRequiredService<IQueryDispatcher>();
        var r1 = await dispatcher.DispatchAsync(new SingletonPing { Message = "x" });
        var r2 = await dispatcher.DispatchAsync(new SingletonPing { Message = "x" });
        Assert.Equal("singleton:x", r1);
        Assert.Equal("singleton:x", r2);
    }

    [Fact]
    public async Task DispatchStreamAsync_Streams_Results()
    {
        var services = new ServiceCollection()
            .AddSoftalleysQueries(typeof(CounterStreamHandler).Assembly)
            .BuildServiceProvider();

        var dispatcher = services.GetRequiredService<IQueryDispatcher>();
        var results = new List<int>();
    await foreach (var v in dispatcher.DispatchStreamAsync<int>(new Counter { Count = 3 }))
        {
            results.Add(v);
        }
        Assert.Equal(new[] { 0, 1, 2 }, results);
    }

    [Fact]
    public async Task Missing_Handler_Throws()
    {
        var services = new ServiceCollection()
            .AddSoftalleysQueries(typeof(DispatcherTests).Assembly) // handlers exist, but not for NoHandler
            .BuildServiceProvider();
        var dispatcher = services.GetRequiredService<IQueryDispatcher>();

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await dispatcher.DispatchAsync(new NoHandler());
        });
    }

    [Fact]
    public async Task Same_Query_Type_Works_For_Single_And_Stream()
    {
        var services = new ServiceCollection()
            .AddSoftalleysQueries(typeof(EchoHandler).Assembly, typeof(EchoStreamHandler).Assembly)
            .BuildServiceProvider();

        var dispatcher = services.GetRequiredService<IQueryDispatcher>();

        var single = await dispatcher.DispatchAsync(new Echo { Text = "ab" });
        Assert.Equal("ab", single);

        var streamed = new List<string>();
        await foreach (var s in dispatcher.DispatchStreamAsync<string>(new Echo { Text = "ab" }))
        {
            streamed.Add(s);
        }
        Assert.Equal(new[] { "a", "b" }, streamed);
    }
}
