using Microsoft.Extensions.DependencyInjection;

namespace Softalleys.Utilities.Commands.Tests;

public class NonGenericSendAsyncTests
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
    public async Task NonGeneric_SendAsync_Returns_Success()
    {
        var services = new ServiceCollection()
            .AddSoftalleysCommands(typeof(AddHandler).Assembly)
            .BuildServiceProvider();
        var mediator = services.GetRequiredService<ICommandMediator>();

        var result = await mediator.SendAsync(new AddCommand(4, 6));
        var success = Assert.IsType<Success>(result);
        Assert.Equal(10, success.Value);
    }

    [Fact]
    public async Task NonGeneric_SendAsync_Returns_Failure()
    {
        var services = new ServiceCollection()
            .AddSoftalleysCommands(typeof(AddHandler).Assembly)
            .BuildServiceProvider();
        var mediator = services.GetRequiredService<ICommandMediator>();

        var result = await mediator.SendAsync(new AddCommand(-1, 3));
        var failure = Assert.IsType<Failure>(result);
        Assert.Equal("Invalid", failure.Title);
    }
}
