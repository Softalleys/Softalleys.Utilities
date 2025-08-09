# Softalleys.Utilities.Queries

Lightweight CQRS query dispatcher with proper DI lifetimes (Scoped by default, opt-in Singleton) and streaming support for .NET 8 and .NET 9.

## Features

- Simple contracts: `IQuery<T>`, `IQueryHandler<TQuery,TResponse>`, `IQueryStreamHandler<TQuery,TResponse>`
- Dispatcher: `IQueryDispatcher` (singleton) to route queries to handlers
- DI-friendly lifetimes:
  - Handlers are Scoped by default
  - Opt-in Singleton via `IQuerySingletonHandler<TQuery,TResponse>` and `IQueryStreamSingletonHandler<TQuery,TResponse>`
- Assembly scanning DI extension: `services.AddSoftalleysQueries(params Assembly[] assemblies)`
- Streaming queries return `IAsyncEnumerable<T>` and keep the scope alive during enumeration

## Installation

Install via NuGet once published:

```pwsh
dotnet add package Softalleys.Utilities.Queries
```

## Concepts

- `IQuery<TResponse>`: marker for a query that produces `TResponse`.
- `IQueryHandler<TQuery,TResponse>`: handles a single-result query.
- `IQueryStreamHandler<TQuery,TResponse>`: handles a streaming query and returns `IAsyncEnumerable<TResponse>`.
- `IQueryDispatcher`: dispatches queries to their registered handlers.
- Optional lifetimes:
  - Scoped (default): `IQueryHandler<,>`, `IQueryStreamHandler<,>`
  - Singleton (opt-in): implement the marker `IQuerySingletonHandler<,>` or `IQueryStreamSingletonHandler<,>` in addition to the base interface.

Constraints and guidance:
- Register exactly one handler per query type. If more than one is registered, the last registration may win when resolving a single service.
- Streaming handlers should be careful to respect cancellation tokens.

## Dependency Injection setup

Register services and scan assemblies that contain handlers:

```csharp
using Softalleys.Utilities.Queries;

// Program.cs
builder.Services.AddSoftalleysQueries(typeof(SomeHandlerInThisAssembly).Assembly);

// Or scan multiple assemblies
builder.Services.AddSoftalleysQueries(
	typeof(HandlerA).Assembly,
	typeof(HandlerB).Assembly
);

// Later resolve IQueryDispatcher wherever you need it
public class MyController(IQueryDispatcher dispatcher) { /* ... */ }
```

The dispatcher creates a new DI scope per dispatch to respect Scoped dependencies. For stream queries, the scope is held for the entire enumeration.

## Usage examples

### Single-result query

```csharp
public record GetUserById(Guid Id) : IQuery<UserDto>;

public class GetUserByIdHandler(MyDbContext db) : IQueryHandler<GetUserById, UserDto>
{
	public async Task<UserDto> HandleAsync(GetUserById query, CancellationToken ct = default)
		=> await db.Users
			.Where(u => u.Id == query.Id)
			.Select(u => new UserDto(u.Id, u.Name))
			.SingleAsync(ct);
}

// Dispatch
var dto = await dispatcher.DispatchAsync(new GetUserById(id), ct);
```

### Streaming query

```csharp
public record GetNumbers(int Count) : IQuery<IAsyncEnumerable<int>>;

public class GetNumbersHandler : IQueryStreamHandler<GetNumbers, int>
{
	public async IAsyncEnumerable<int> HandleAsync(GetNumbers query, [EnumeratorCancellation] CancellationToken ct = default)
	{
		for (var i = 0; i < query.Count; i++)
		{
			ct.ThrowIfCancellationRequested();
			yield return i;
			await Task.Yield();
		}
	}
}

// Dispatch
await foreach (var n in dispatcher.DispatchStreamAsync<int>(new GetNumbers(5), ct))
{
	Console.WriteLine(n);
}
```

### Opt-in Singleton handler

```csharp
public record GetVersion() : IQuery<string>;

public class GetVersionHandler : IQuerySingletonHandler<GetVersion, string>
{
	public Task<string> HandleAsync(GetVersion query, CancellationToken ct = default)
		=> Task.FromResult("1.0.0");
}
```

## Error handling

- If no handler is registered for a given query, the dispatcher throws `InvalidOperationException`.
- It’s recommended to keep query handlers side-effect free. Use commands (not included here) for writes.

## Testing tips

- Handlers are plain classes and easy to unit test in isolation.
- The dispatcher can be integration-tested by composing a minimal `ServiceCollection`, calling `AddSoftalleysQueries`, and asserting behavior.

## Packaging and publishing

This project is multi-targeted for .NET 8 and .NET 9 and includes conditional dependencies. It also ships a `.nuspec` file if you prefer packing from nuspec. Typical flows:

Option A: pack via the project file

```pwsh
dotnet pack .\Softalleys.Utilities.Queries\Softalleys.Utilities.Queries.csproj -c Release
```

Option B: pack via the nuspec (requires NuGet CLI)

```pwsh
nuget pack .\Softalleys.Utilities.Queries\Softalleys.Utilities.Queries.nuspec -Properties Configuration=Release
```

Then push:

```pwsh
dotnet nuget push .\Softalleys.Utilities.Queries\bin\Release\Softalleys.Utilities.Queries.*.nupkg --source https://api.nuget.org/v3/index.json --api-key <YOUR_KEY>
```

---

## Objectives (design summary)

- `IQuery<T>` marker interface
- `IQueryHandler<TQuery,TResponse>` and `IQueryStreamHandler<TQuery,TResponse>`
- `IQueryDispatcher` (singleton)
- Scoped default lifetimes; opt-in singleton via marker interfaces
- DI assembly scanning via `AddSoftalleysQueries`
- Streaming support with scope preserved over enumeration
