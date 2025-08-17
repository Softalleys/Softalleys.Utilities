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

- Mediator is registered as Singleton
- Handlers are Scoped by default (Singleton if `ICommandSingletonHandler<,>` is implemented)
- Validators, processors, and post-actions are Scoped by default

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
    # Softalleys.Utilities.Commands

    Lightweight command pipeline for .NET (net8/net9) with validators, processors, default handler, post-actions and DI scanning.

    Key features
    - ICommand/ICommandHandler pipeline with validation and processing stages
    - DefaultCommandHandler to orchestrate validation → processing → post-actions
    - DI scanning for handlers, validators, processors and post-actions
    - Scoped and Singleton handler support via marker interface

    Install
    ```
    dotnet add package Softalleys.Utilities.Commands --version 1.0.0
    ```

    Quickstart
    ```csharp
    // Register
    builder.Services.AddSoftalleysCommands(typeof(SomeHandler).Assembly);

    // Define command
    public record CreateUserCommand(string Email, string Password) : ICommand<CreateUserResult>;

    // Send
    var result = await mediator.SendAsync<CreateUserResult, CreateUserCommand>(new CreateUserCommand("a@b.com","pwd"));
    ```

    License
    MIT

    Source
    https://github.com/Softalleys/Softalleys.Utilities

    For more details and examples, see the project README in the repository.
{
