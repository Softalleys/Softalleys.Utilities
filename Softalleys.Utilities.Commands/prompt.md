# Softalleys.Utilities.Commands

On most of Softalley's .NET Projects we use Command and Query Pattern for most of the operations. we dont want to use external libraries so we made our own library Softalleys.Utilities.Events and Softalleys.Utilities.Queries on @Softalleys/Softalleys.Utilities to implement Event-Driven Pattern and Query-Driven Pattern similar to libraries like MediatR. I want to implement a solution for Commands.

Currently I use Commands, Results, Handlers, Processors, and Validators to process a concret task operation that make changes on the system.  E.g.

```c#
public record CreateCategoryCommand(string Name, string Description, string Icon);
```

```c#
/// <summary>
/// Result of a create category operation.
/// </summary>
public abstract record CreateCategoryResult;

/// <summary>
/// An implementation of the result that means that the Command is valid and passes all validations
/// </summary>
public record CreateCategoryValid : CreateCategoryResult;

/// <summary>
/// Indicates a successful category creation.
/// </summary>
public record CreateCategorySuccessResult(Category Category) : CreateCategoryResult;

/// <summary>
/// Indicates a failure during category creation.
/// </summary>
/// <param name="Title">A short title for the error.</param>
/// <param name="Message">A detailed error message.</param>
public record CreateCategoryFailureResult(string Title, string Message)
    : CreateCategoryResult, IErrorResponse
{
    public string Error { get; init; } = "Bad Request";
    /// <inheritdoc/>
    public int Status { get; init; } = 400;
    /// <inheritdoc/>
    public IDictionary<string, string[]> Errors { get; init; } = new Dictionary<string, string[]>();
    /// <inheritdoc/>
    public string? TraceId { get; init; }
}
```

The standard process for handling commands involves several key steps:

1. **Command Creation**: A command is created, typically as a record or class, encapsulating all necessary data for the operation.

2. **Validation**: Before processing, the command is validated to ensure all required fields are present and correctly formatted. This may involve a dedicated validator class or inline validation logic. Normally the validator returns a abstract record or class that can be inherit by the command valid result or the command failure result. 

3. **Command Processing**: A command processor is responsible for executing the command. It retrieves any necessary data, performs business logic, persists the data, and produces a result.

4. **Command Handler**: The handler orchestrates the overall command processing, first using the validator to validate the command. If the command is valid, it is passed to the command processor for execution. If the processor continue with a success result do an optional step calling the Event Bus, print logs, etc. then return the result.

5. **Event Publishing**: If the command results in a significant state change, an event may be published to notify other parts of the system using the Softalleys.Utilities.Events library or any other event publishing mechanism. The Softalleys.Utilities.Commands must be agnostic to the specific event publishing implementation.

6. **Standard Result Handling**: The Handler, Processor, and Validator should all adhere to a standard result format, ensuring consistency across the application. The three must use the same abstract record or class result and return a proper result that represents the proper outcome. The possible results could be valid (when the validator approve the command but do not do any bussiness logic or presists the data yet), success (when the command is processed successfully and the data is persisted or the bussiness logic is applied), or failure (when the command processing encounters an error in any step of the pipeline).

7. **Error Result Standard**: The error results must implement the `IErrorResponse` interface, providing a consistent structure for error responses across the application.

8. **Result Enrichment**: Additional metadata may be included in the result, such as the user ID, timestamp, persisted entity, etc.

9. **Command Mediator Sender**: The command mediator is responsible for sending the command to the appropriate handler. It abstracts the details of the command handling process, allowing for a more decoupled architecture.

10. **Command Pipeline**: The command pipeline is the overall flow of the command from creation to completion. It encompasses all the steps outlined above, ensuring that each step is executed in the correct order and that the results are properly handled. The pipeline may be handle manually by the developer on the Command Handler, so it is desicion of the developer calling the validator or processor.

11. **Command Interfaces and Abstractions**: The command handling process should be built around well-defined interfaces and abstractions. This allows for easier testing, mocking, and swapping of implementations without affecting the overall system.

12. **Autodiscovery and Registration**: The command handling components (validators, processors, handlers) should be automatically discovered and registered within the application. This can be achieved through conventions, attributes, or configuration, reducing the need for manual wiring and promoting a more modular architecture.

By following this process, we can ensure that commands are handled consistently and effectively across the application.