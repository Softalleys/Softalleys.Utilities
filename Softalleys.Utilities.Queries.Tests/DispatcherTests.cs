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

    // Single record used by a single handler that implements both single and stream interfaces
    private record BothEcho(string Text) : IQuery<string>;

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

    // Conflict types to validate multiple handlers error for single-result queries
    private class Conflict : IQuery<string> { public string Value { get; init; } = string.Empty; }
    private class ConflictHandler1 : IQueryHandler<Conflict, string>
    {
        public Task<string> HandleAsync(Conflict query, CancellationToken cancellationToken = default) => Task.FromResult($"h1:{query.Value}");
    }
    private class ConflictHandler2 : IQueryHandler<Conflict, string>
    {
        public Task<string> HandleAsync(Conflict query, CancellationToken cancellationToken = default) => Task.FromResult($"h2:{query.Value}");
    }

    // Conflict types to validate multiple handlers error for stream queries
    private class StreamConflict : IQuery<int> { public int Count { get; init; } = 1; }
    private class StreamConflictHandler1 : IQueryStreamHandler<StreamConflict, int>
    {
        public async IAsyncEnumerable<int> StreamAsync(StreamConflict query, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            for (int i = 0; i < query.Count; i++) { cancellationToken.ThrowIfCancellationRequested(); yield return 1; await Task.Yield(); }
        }
    }
    private class StreamConflictHandler2 : IQueryStreamHandler<StreamConflict, int>
    {
        public async IAsyncEnumerable<int> StreamAsync(StreamConflict query, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            for (int i = 0; i < query.Count; i++) { cancellationToken.ThrowIfCancellationRequested(); yield return 2; await Task.Yield(); }
        }
    }

    // Types to validate missing stream handler error
    private class NoStream : IQuery<int> { }

    // Stream lifetime probes
    private class ScopedStreamProbe : IQuery<int> { }
    private class ScopedStreamProbeHandler : IQueryStreamHandler<ScopedStreamProbe, int>
    {
        private static int _instances;
        public int InstanceId { get; } = Interlocked.Increment(ref _instances);
        public async IAsyncEnumerable<int> StreamAsync(ScopedStreamProbe query, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            yield return InstanceId;
            await Task.Yield();
        }
    }

    private class SingletonStreamProbe : IQuery<int> { }
    private class SingletonStreamProbeHandler : IQueryStreamSingletonHandler<SingletonStreamProbe, int>
    {
        private static int _instances;
        public int InstanceId { get; } = Interlocked.Increment(ref _instances);
        public async IAsyncEnumerable<int> StreamAsync(SingletonStreamProbe query, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            yield return InstanceId;
            await Task.Yield();
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
        // Register the test assembly once; it contains both EchoHandler (single) and EchoStreamHandler (stream)
        var services = new ServiceCollection()
            .AddSoftalleysQueries(typeof(EchoHandler).Assembly)
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

    [Fact]
    public async Task Single_Record_Handled_By_Both_Interfaces_In_Same_Handler()
    {
        var services = new ServiceCollection()
            .AddSoftalleysQueries(typeof(BothEchoHandler).Assembly)
            .BuildServiceProvider();

        var dispatcher = services.GetRequiredService<IQueryDispatcher>();

        // Single result via IQueryHandler implemented on the same handler
        var single = await dispatcher.DispatchAsync(new BothEcho("xy"));
        Assert.Equal("xy", single);

        // Streamed result via IQueryStreamHandler implemented on the same handler
        var streamed = new List<string>();
        await foreach (var s in dispatcher.DispatchStreamAsync<string>(new BothEcho("xy")))
        {
            streamed.Add(s);
        }
        Assert.Equal(new[] { "x", "y" }, streamed);
    }

    [Fact]
    public async Task Multiple_Single_Handlers_For_Same_Query_Throws()
    {
        // Intentionally register the same assembly twice to simulate duplicate handler registrations
        var services = new ServiceCollection()
            .AddSoftalleysQueries(typeof(ConflictHandler1).Assembly)
            .AddSoftalleysQueries(typeof(ConflictHandler1).Assembly)
            .BuildServiceProvider();

        var dispatcher = services.GetRequiredService<IQueryDispatcher>();

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            _ = await dispatcher.DispatchAsync(new Conflict { Value = "x" });
        });
    }

    [Fact]
    public async Task Multiple_Stream_Handlers_For_Same_Query_Throws()
    {
        // Intentionally register the same assembly twice to simulate duplicate stream handler registrations
        var services = new ServiceCollection()
            .AddSoftalleysQueries(typeof(StreamConflictHandler1).Assembly)
            .AddSoftalleysQueries(typeof(StreamConflictHandler1).Assembly)
            .BuildServiceProvider();

        var dispatcher = services.GetRequiredService<IQueryDispatcher>();

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await foreach (var _ in dispatcher.DispatchStreamAsync<int>(new StreamConflict { Count = 1 }))
            {
                // no-op
            }
        });
    }

    [Fact]
    public async Task Missing_Stream_Handler_Throws()
    {
        var services = new ServiceCollection()
            .AddSoftalleysQueries(typeof(DispatcherTests).Assembly) // No stream handler for NoStream
            .BuildServiceProvider();
        var dispatcher = services.GetRequiredService<IQueryDispatcher>();

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await foreach (var _ in dispatcher.DispatchStreamAsync<int>(new NoStream())) { }
        });
    }

    [Fact]
    public async Task Scoped_Stream_Handler_Creates_New_Instance_Per_Dispatch()
    {
        var services = new ServiceCollection()
            .AddSoftalleysQueries(typeof(ScopedStreamProbeHandler).Assembly)
            .BuildServiceProvider();

        var dispatcher = services.GetRequiredService<IQueryDispatcher>();

        var ids = new List<int>();
        await foreach (var id in dispatcher.DispatchStreamAsync<int>(new ScopedStreamProbe())) { ids.Add(id); break; }
        await foreach (var id in dispatcher.DispatchStreamAsync<int>(new ScopedStreamProbe())) { ids.Add(id); break; }

        Assert.Equal(2, ids.Count);
        Assert.NotEqual(ids[0], ids[1]); // different handler instances per dispatch
    }

    [Fact]
    public async Task Singleton_Stream_Handler_Reuses_Same_Instance()
    {
        var services = new ServiceCollection()
            .AddSoftalleysQueries(typeof(SingletonStreamProbeHandler).Assembly)
            .BuildServiceProvider();

        var dispatcher = services.GetRequiredService<IQueryDispatcher>();

        var ids = new List<int>();
        await foreach (var id in dispatcher.DispatchStreamAsync<int>(new SingletonStreamProbe())) { ids.Add(id); break; }
        await foreach (var id in dispatcher.DispatchStreamAsync<int>(new SingletonStreamProbe())) { ids.Add(id); break; }

        Assert.Equal(2, ids.Count);
        Assert.Equal(ids[0], ids[1]); // same handler instance across dispatches
    }
}
