using Microsoft.Extensions.DependencyInjection;
using Softalleys.Utilities.Commands;

abstract record CreateUserResult;
record CreateUserValid : CreateUserResult, IValidationStageResult { public bool Continue => true; }
record CreateUserSuccess(Guid Id, string Email) : CreateUserResult;
record CreateUserFailure(string Title, string Message) : CreateUserResult, Softalleys.Utilities.Interfaces.IErrorResponse
{
    public string Error { get; init; } = "Bad Request";
    public int Status { get; init; } = 400;
    public IDictionary<string, string[]> Errors { get; init; } = new Dictionary<string, string[]>();
    public string? TraceId { get; init; }
}

record CreateUserCommand(string Email, string Password) : ICommand<CreateUserResult>;

class CreateUserValidator : ICommandValidator<CreateUserCommand, CreateUserResult>
{
    public Task<CreateUserResult> ValidateAsync(CreateUserCommand c, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(c.Email)) return Task.FromResult<CreateUserResult>(new CreateUserFailure("Email required","Email is mandatory"));
        if (!c.Email.Contains('@')) return Task.FromResult<CreateUserResult>(new CreateUserFailure("Invalid email","Format"));
        if (string.IsNullOrWhiteSpace(c.Password) || c.Password.Length < 6) return Task.FromResult<CreateUserResult>(new CreateUserFailure("Weak password","Min length 6"));
        return Task.FromResult<CreateUserResult>(new CreateUserValid());
    }
}

class CreateUserProcessor : ICommandProcessor<CreateUserCommand, CreateUserResult>
{
    public Task<CreateUserResult> ProcessAsync(CreateUserCommand c, CancellationToken ct = default)
        => Task.FromResult<CreateUserResult>(new CreateUserSuccess(Guid.NewGuid(), c.Email));
}

class CreateUserHandler : DefaultCommandHandler<CreateUserCommand, CreateUserResult>
{
    public CreateUserHandler(ICommandValidator<CreateUserCommand, CreateUserResult> v,
        ICommandProcessor<CreateUserCommand, CreateUserResult> p) : base(v, p) { }
}

class Program
{
    static async Task Main()
    {
        var services = new ServiceCollection()
            .AddSoftalleysCommands(typeof(Program).Assembly)
            .BuildServiceProvider();

        var mediator = services.GetRequiredService<ICommandMediator>();

    var ok = await mediator.SendAsync<CreateUserResult, CreateUserCommand>(new CreateUserCommand("user@example.com","secret!"));
        Console.WriteLine(ok);
    var bad = await mediator.SendAsync<CreateUserResult, CreateUserCommand>(new CreateUserCommand("","123"));
        Console.WriteLine(bad);
    }
}
