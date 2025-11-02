# Softalleys.Utilities.Commands

Lightweight command pipeline for .NET 8/9 with validators, processors, handlers, optional post-actions, and DI scanning. Designed to pair with Softalleys.Utilities.Events and Softalleys.Utilities.Queries.

## Concepts

- `ICommand<TResult>`: marker for a command that yields a domain-specific `TResult` discriminated union (record/class hierarchy)
- `ICommandValidator<TCommand,TResult>`: validates the command and returns a `TResult`. If the result indicates "valid" the pipeline continues; otherwise, returns failure immediately
- `ICommandProcessor<TCommand,TResult>`: performs business logic and persistence, returning a `TResult` (usually success/failure variants)
- `ICommandHandler<TCommand,TResult>`: orchestrates the pipeline. You can implement your own or use `DefaultCommandHandler<TCommand,TResult>`
- `ICommandPostAction<TCommand,TResult>`: optional post-processing hook (publish events, logging, audit, etc.)
- `ICommandSingletonHandler<TCommand,TResult>`: marker to opt-in Singleton lifetime instead of default Scoped
- `ICommandMediator`: sends commands to their handlers with proper scope

## Installation

Once published to NuGet:

```pwsh
dotnet add package Softalleys.Utilities.Commands
```

## Dependency Injection

Register and scan assemblies that contain handlers/validators/processors/post-actions:

```csharp
using Softalleys.Utilities.Commands;

builder.Services.AddSoftalleysCommands(typeof(SomeHandler).Assembly);
```

- Mediator is registered as Scoped (safe with scoped dependencies like DbContext)
- Handlers are Scoped by default (Singleton if `ICommandSingletonHandler<,>` is implemented)
- Validators, processors, and post-actions are Scoped by default

### Inject single or multiple validators/processors (new)

You can inject either a single service or a collection/array:

- Single: `ICommandValidator<TCommand,TResult>` or `ICommandProcessor<TCommand,TResult>`
- Multiple: `IEnumerable<ICommandValidator<TCommand,TResult>>` or `IEnumerable<ICommandProcessor<TCommand,TResult>>`
- Multiple (array): `ICommandValidator<TCommand,TResult>[]` or `ICommandProcessor<TCommand,TResult>[]`

The container will resolve all registered implementations in assembly scanning order. For arrays, the library registers adapters so arrays materialize correctly even when empty.

## Using the Default Pipeline

Define your command and result union:

```csharp
public record CreateCategoryCommand(string Name, string Description, string Icon) : ICommand<CreateCategoryResult>;

public abstract record CreateCategoryResult;
public record CreateCategoryValid : CreateCategoryResult;
public record CreateCategorySuccessResult(Category Category) : CreateCategoryResult;
public record CreateCategoryFailureResult(string Title, string Message)
    : CreateCategoryResult, Softalleys.Utilities.Interfaces.IErrorResponse
{
    public string Error { get; init; } = "Bad Request";
    public int Status { get; init; } = 400;
    public IDictionary<string, string[]> Errors { get; init; } = new Dictionary<string, string[]>();
    public string? TraceId { get; init; }
}
```

Implement validator and processor:

```csharp
public class CreateCategoryValidator : ICommandValidator<CreateCategoryCommand, CreateCategoryResult>
{
    public Task<CreateCategoryResult> ValidateAsync(CreateCategoryCommand cmd, CancellationToken ct = default)
```

### Injecting multiple validators and processors in a handler

```csharp
public record VerifyAlertCommand(CarMetadata Metadata) : ICommand<VerifyAlertResult>;
public record VerifyAlertResult(bool IsAlerted, string? TypeOfAlert) : IValidationStageResult
{
    public bool Continue => !IsAlerted; // short-circuit if an alert is found
}

public class VerifyAlertCommandModelValidator : ICommandValidator<VerifyAlertCommand, VerifyAlertResult> { /* ... */ }
public class VerifyAlertCommandPaymentValidator : ICommandValidator<VerifyAlertCommand, VerifyAlertResult> { /* ... */ }

public class VerifyAlertCommandSpeedProcessor : ICommandProcessor<VerifyAlertCommand, VerifyAlertResult> { /* ... */ }
public class VerifyAlertCommandLicenseStatusProcessor : ICommandProcessor<VerifyAlertCommand, VerifyAlertResult> { /* ... */ }
public class VerifyAlertCommandMaintenanceProcessor : ICommandProcessor<VerifyAlertCommand, VerifyAlertResult> { /* ... */ }

// Handler can inject arrays or IEnumerable
public class VerifyAlertCommandHandler(
    ICommandProcessor<VerifyAlertCommand, VerifyAlertResult>[] processors,
    ICommandValidator<VerifyAlertCommand, VerifyAlertResult>[] validators)
    : ICommandHandler<VerifyAlertCommand, VerifyAlertResult>
{
    public async Task<VerifyAlertResult> HandleAsync(VerifyAlertCommand command, CancellationToken cancellationToken = default)
    {
        foreach (var v in validators)
        {
            var vr = await v.ValidateAsync(command, cancellationToken);
            if (vr.IsAlerted) return vr; // short-circuit on first alert
        }
        foreach (var p in processors)
        {
            var pr = await p.ProcessAsync(command, cancellationToken);
            if (pr.IsAlerted) return pr; // short-circuit on first alert
        }
        return new VerifyAlertResult(false, null);
    }
}
```

You can also inject only a single validator/processor if you prefer:

```csharp
public class SingleValidatorHandler(
    ICommandValidator<MyCommand, MyResult> validator,
    ICommandProcessor<MyCommand, MyResult> processor) : ICommandHandler<MyCommand, MyResult>
{
    public async Task<MyResult> HandleAsync(MyCommand command, CancellationToken ct = default)
    {
        var vr = await validator.ValidateAsync(command, ct);
        if (vr is IValidationStageResult ok && !ok.Continue) return vr;
        return await processor.ProcessAsync(command, ct);
    }
}
```
