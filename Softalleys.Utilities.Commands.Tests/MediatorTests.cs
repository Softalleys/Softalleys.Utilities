using Microsoft.Extensions.DependencyInjection;

namespace Softalleys.Utilities.Commands.Tests;

public class MediatorTests
{
    private abstract record ResultBase;
    private record Valid : ResultBase, IValidationStageResult { public bool Continue => true; }
    private record Success(int Value) : ResultBase;
    private record Failure(string Title, string Message) : ResultBase, Interfaces.IErrorResponse
    {
        public string Error { get; init; } = "Bad Request";
        public int Status { get; init; } = 400;
        public IDictionary<string, string[]> Errors { get; init; } = new Dictionary<string, string[]>();
        public string? TraceId { get; init; }
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

    [Fact]
    public async Task Mediator_Returns_Success_When_Valid()
    {
        var services = new ServiceCollection()
            .AddSoftalleysCommands(typeof(AddHandler).Assembly)
            .BuildServiceProvider();
        var mediator = services.GetRequiredService<ICommandMediator>();

    var result = await mediator.SendAsync<ResultBase, AddCommand>(new AddCommand(2, 3));
        var success = Assert.IsType<Success>(result);
        Assert.Equal(5, success.Value);
    }

    [Fact]
    public async Task Mediator_Returns_Failure_When_Invalid()
    {
        var services = new ServiceCollection()
            .AddSoftalleysCommands(typeof(AddHandler).Assembly)
            .BuildServiceProvider();
        var mediator = services.GetRequiredService<ICommandMediator>();

    var result = await mediator.SendAsync<ResultBase, AddCommand>(new AddCommand(-1, 3));
        var failure = Assert.IsType<Failure>(result);
        Assert.Equal("Invalid", failure.Title);
    }

    private record SingleHandlerCommand(string Text) : ICommand<string>;
    private class SingleHandler : ICommandHandler<SingleHandlerCommand, string>
    {
        public Task<string> HandleAsync(SingleHandlerCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult(command.Text.ToUpperInvariant());
    }

    [Fact]
    public async Task Single_Handler_Execution_Works()
    {
        var services = new ServiceCollection()
            .AddSoftalleysCommands(typeof(SingleHandler).Assembly)
            .BuildServiceProvider();
        var mediator = services.GetRequiredService<ICommandMediator>();

    var result = await mediator.SendAsync<string, SingleHandlerCommand>(new SingleHandlerCommand("abc"));
        Assert.Equal("ABC", result);
    }

    private record ConflictCmd(int X) : ICommand<int>;
    private class ConflictH1 : ICommandHandler<ConflictCmd, int>
    {
        public Task<int> HandleAsync(ConflictCmd command, CancellationToken cancellationToken = default) => Task.FromResult(command.X + 1);
    }
    private class ConflictH2 : ICommandHandler<ConflictCmd, int>
    {
        public Task<int> HandleAsync(ConflictCmd command, CancellationToken cancellationToken = default) => Task.FromResult(command.X + 2);
    }

    [Fact]
    public async Task Multiple_Handlers_Throws()
    {
        var services = new ServiceCollection()
            .AddSoftalleysCommands(typeof(ConflictH1).Assembly)
            .AddSoftalleysCommands(typeof(ConflictH2).Assembly)
            .BuildServiceProvider();
        var mediator = services.GetRequiredService<ICommandMediator>();

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            _ = await mediator.SendAsync<int, ConflictCmd>(new ConflictCmd(1));
        });
    }

    // Test multiple validators and processors
    private record MultiCommand(int Value, string Text) : ICommand<MultiResult>;
    private abstract record MultiResult;
    private record MultiValid : MultiResult, IValidationStageResult { public bool Continue => true; }
    private record MultiSuccess(int ProcessedValue, string ProcessedText) : MultiResult;
    private record MultiFailure(string Reason) : MultiResult, Interfaces.IErrorResponse
    {
        public string Error { get; init; } = "Bad Request";
        public string Title { get; init; } = "Validation Error";
        public string Message { get; init; } = Reason;
        public int Status { get; init; } = 400;
        public IDictionary<string, string[]> Errors { get; init; } = new Dictionary<string, string[]>();
        public string? TraceId { get; init; }
    }

    // First validator checks value range
    private class RangeValidator : ICommandValidator<MultiCommand, MultiResult>
    {
        public Task<MultiResult> ValidateAsync(MultiCommand command, CancellationToken cancellationToken = default)
            => command.Value < 0 || command.Value > 100
                ? Task.FromResult<MultiResult>(new MultiFailure("Value must be between 0 and 100"))
                : Task.FromResult<MultiResult>(new MultiValid());
    }

    // Second validator checks text length
    private class LengthValidator : ICommandValidator<MultiCommand, MultiResult>
    {
        public Task<MultiResult> ValidateAsync(MultiCommand command, CancellationToken cancellationToken = default)
            => string.IsNullOrEmpty(command.Text) || command.Text.Length < 3
                ? Task.FromResult<MultiResult>(new MultiFailure("Text must be at least 3 characters"))
                : Task.FromResult<MultiResult>(new MultiValid());
    }

    // First processor doubles the value
    private class DoublingProcessor : ICommandProcessor<MultiCommand, MultiResult>
    {
        public Task<MultiResult> ProcessAsync(MultiCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult<MultiResult>(new MultiSuccess(command.Value * 2, command.Text));
    }

    // Second processor converts text to uppercase (this will be the final result)
    private class UppercaseProcessor : ICommandProcessor<MultiCommand, MultiResult>
    {
        public Task<MultiResult> ProcessAsync(MultiCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult<MultiResult>(new MultiSuccess(command.Value, command.Text.ToUpper()));
    }

    // Custom handler that injects arrays of validators and processors
    private class CustomMultiHandler : ICommandHandler<MultiCommand, MultiResult>
    {
        private readonly IEnumerable<ICommandValidator<MultiCommand, MultiResult>> _validators;
        private readonly IEnumerable<ICommandProcessor<MultiCommand, MultiResult>> _processors;

        public CustomMultiHandler(
            IEnumerable<ICommandValidator<MultiCommand, MultiResult>> validators,
            IEnumerable<ICommandProcessor<MultiCommand, MultiResult>> processors)
        {
            _validators = validators ?? Array.Empty<ICommandValidator<MultiCommand, MultiResult>>();
            _processors = processors ?? throw new ArgumentException("At least one processor is required");
            if (!_processors.Any())
                throw new ArgumentException("At least one processor is required");
        }

        public async Task<MultiResult> HandleAsync(MultiCommand command, CancellationToken cancellationToken = default)
        {
            // Run validators
            foreach (var validator in _validators)
            {
                var validationResult = await validator.ValidateAsync(command, cancellationToken).ConfigureAwait(false);
                if (validationResult is not IValidationStageResult valid || !valid.Continue)
                {
                    return validationResult;
                }
            }

            // Run processors, last one wins
            MultiResult result = default!;
            foreach (var processor in _processors)
            {
                result = await processor.ProcessAsync(command, cancellationToken).ConfigureAwait(false);
            }

            return result;
        }
    }

    [Fact]
    public async Task Multiple_Validators_All_Pass()
    {
        var services = new ServiceCollection()
            .AddSoftalleysCommands(typeof(RangeValidator).Assembly)
            .BuildServiceProvider();
        var mediator = services.GetRequiredService<ICommandMediator>();

        var result = await mediator.SendAsync<MultiResult, MultiCommand>(new MultiCommand(50, "Hello"));
        
        // Should get result from the last processor (UppercaseProcessor)
        var success = Assert.IsType<MultiSuccess>(result);
        Assert.Equal(50, success.ProcessedValue);
        Assert.Equal("HELLO", success.ProcessedText);
    }

    [Fact]
    public async Task Multiple_Validators_First_Fails()
    {
        var services = new ServiceCollection()
            .AddSoftalleysCommands(typeof(RangeValidator).Assembly)
            .BuildServiceProvider();
        var mediator = services.GetRequiredService<ICommandMediator>();

        var result = await mediator.SendAsync<MultiResult, MultiCommand>(new MultiCommand(150, "Hello"));
        
        // Should fail on first validator (range check)
        var failure = Assert.IsType<MultiFailure>(result);
        Assert.Equal("Value must be between 0 and 100", failure.Reason);
    }

    [Fact]
    public async Task Multiple_Validators_Second_Fails()
    {
        var services = new ServiceCollection()
            .AddSoftalleysCommands(typeof(RangeValidator).Assembly)
            .BuildServiceProvider();
        var mediator = services.GetRequiredService<ICommandMediator>();

        var result = await mediator.SendAsync<MultiResult, MultiCommand>(new MultiCommand(50, "Hi"));
        
        // Should fail on second validator (length check)
        var failure = Assert.IsType<MultiFailure>(result);
        Assert.Equal("Text must be at least 3 characters", failure.Reason);
    }

    [Fact]
    public async Task Multiple_Processors_Last_One_Wins()
    {
        var services = new ServiceCollection()
            .AddSoftalleysCommands(typeof(RangeValidator).Assembly)
            .BuildServiceProvider();
        var mediator = services.GetRequiredService<ICommandMediator>();

        var result = await mediator.SendAsync<MultiResult, MultiCommand>(new MultiCommand(5, "test"));
        
        // Should get result from the last processor (UppercaseProcessor)
        // The DoubleProcessor would return (10, "test") but UppercaseProcessor should override with (5, "TEST")
        var success = Assert.IsType<MultiSuccess>(result);
        Assert.Equal(5, success.ProcessedValue);  // Original value, not doubled
        Assert.Equal("TEST", success.ProcessedText);  // Uppercased
    }

    [Fact]
    public void DefaultCommandHandler_Throws_When_No_Processors()
    {
        // Test that DefaultCommandHandler requires at least one processor
        Assert.Throws<ArgumentException>(() =>
        {
            new DefaultCommandHandler<MultiCommand, MultiResult>(
                new[] { new RangeValidator() },
                Array.Empty<ICommandProcessor<MultiCommand, MultiResult>>());
        });
    }

    [Fact]
    public async Task DefaultCommandHandler_Works_With_No_Validators()
    {
        // Test that validators are optional
        var handler = new DefaultCommandHandler<MultiCommand, MultiResult>(
            null, // No validators
            new[] { new DoublingProcessor() });

        var result = await handler.HandleAsync(new MultiCommand(5, "test"), CancellationToken.None);
        
        var success = Assert.IsType<MultiSuccess>(result);
        Assert.Equal(10, success.ProcessedValue);  // Should be doubled
        Assert.Equal("test", success.ProcessedText);
    }

    [Fact]
    public async Task Custom_Handler_With_Multiple_Validators_And_Processors()
    {
        var services = new ServiceCollection()
            .AddSoftalleysCommands(typeof(CustomMultiHandler).Assembly)
            .BuildServiceProvider();
        var mediator = services.GetRequiredService<ICommandMediator>();

        var result = await mediator.SendAsync<MultiResult, MultiCommand>(new MultiCommand(25, "world"));
        
        // Should pass all validators and get result from the last processor (UppercaseProcessor)
        var success = Assert.IsType<MultiSuccess>(result);
        Assert.Equal(25, success.ProcessedValue);  // Original value, not doubled
        Assert.Equal("WORLD", success.ProcessedText);  // Uppercased
    }
}
