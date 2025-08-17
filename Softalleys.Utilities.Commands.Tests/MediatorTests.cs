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
}
