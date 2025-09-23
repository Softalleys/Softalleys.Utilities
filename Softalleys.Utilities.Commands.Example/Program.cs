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

        var ok = await mediator.SendAsync<CreateUserResult, CreateUserCommand>(new CreateUserCommand("user@example.com", "secret!"));
        Console.WriteLine(ok);
        var bad = await mediator.SendAsync<CreateUserResult, CreateUserCommand>(new CreateUserCommand("", "123"));
        Console.WriteLine(bad);

        // Non-generic overload: useful when you only have ICommand<TResult> at compile time
        ICommand<CreateUserResult> cmd = new CreateUserCommand("user2@example.com", "pwd123");
        var ok2 = await mediator.SendAsync<CreateUserResult>(cmd);
        Console.WriteLine(ok2);

        // ----- VerifyAlert scenario: multiple validators and processors injected as arrays -----
        var noAlert = await mediator.SendAsync<VerifyAlertResult, VerifyAlertCommand>(
            new VerifyAlertCommand(new CarMetadata(50, IsLicenseSuspended: false, NeedsMaintenance: false, PaymentOverdue: false, Model: "Civic")));
        Console.WriteLine($"NoAlert => {noAlert.IsAlerted} | {noAlert.TypeOfAlert ?? "-"}");

        var speedAlert = await mediator.SendAsync<VerifyAlertResult, VerifyAlertCommand>(
            new VerifyAlertCommand(new CarMetadata(140, IsLicenseSuspended: false, NeedsMaintenance: false, PaymentOverdue: false, Model: "Civic")));
        Console.WriteLine($"SpeedAlert => {speedAlert.IsAlerted} | {speedAlert.TypeOfAlert}");

        var licenseAlert = await mediator.SendAsync<VerifyAlertResult, VerifyAlertCommand>(
            new VerifyAlertCommand(new CarMetadata(70, IsLicenseSuspended: true, NeedsMaintenance: false, PaymentOverdue: false, Model: "Civic")));
        Console.WriteLine($"LicenseAlert => {licenseAlert.IsAlerted} | {licenseAlert.TypeOfAlert}");

        var modelAlert = await mediator.SendAsync<VerifyAlertResult, VerifyAlertCommand>(
            new VerifyAlertCommand(new CarMetadata(70, IsLicenseSuspended: false, NeedsMaintenance: false, PaymentOverdue: false, Model: "BannedModel")));
        Console.WriteLine($"ModelAlert => {modelAlert.IsAlerted} | {modelAlert.TypeOfAlert}");
    }
}

// ----------------- VerifyAlert domain showcasing array injection -----------------

public record CarMetadata(int SpeedKmh, bool IsLicenseSuspended, bool NeedsMaintenance, bool PaymentOverdue, string Model);

public record VerifyAlertCommand(CarMetadata Metadata) : ICommand<VerifyAlertResult>;

// We can embed continuation semantics in the TResult via IValidationStageResult
public record VerifyAlertResult(bool IsAlerted, string? TypeOfAlert) : IValidationStageResult
{
    // Continue validation/processing only when no alert has been found yet
    public bool Continue => !IsAlerted;
}

// Validators: can short-circuit (return IsAlerted=true) or allow to continue (IsAlerted=false)
public class VerifyAlertCommandModelValidator : ICommandValidator<VerifyAlertCommand, VerifyAlertResult>
{
    private static readonly HashSet<string> BannedModels = new(StringComparer.OrdinalIgnoreCase) { "BannedModel", "PrototypeX" };

    public Task<VerifyAlertResult> ValidateAsync(VerifyAlertCommand command, CancellationToken cancellationToken = default)
    {
        var alerted = BannedModels.Contains(command.Metadata.Model);
        return Task.FromResult(new VerifyAlertResult(alerted, alerted ? "Model" : null));
    }
}

public class VerifyAlertCommandPaymentValidator : ICommandValidator<VerifyAlertCommand, VerifyAlertResult>
{
    public Task<VerifyAlertResult> ValidateAsync(VerifyAlertCommand command, CancellationToken cancellationToken = default)
    {
        var alerted = command.Metadata.PaymentOverdue;
        return Task.FromResult(new VerifyAlertResult(alerted, alerted ? "Payment" : null));
    }
}

// Processors: check additional alert conditions; handler stops on first alert found
public class VerifyAlertCommandSpeedProcessor : ICommandProcessor<VerifyAlertCommand, VerifyAlertResult>
{
    public Task<VerifyAlertResult> ProcessAsync(VerifyAlertCommand command, CancellationToken cancellationToken = default)
    {
        var alerted = command.Metadata.SpeedKmh > 120;
        return Task.FromResult(new VerifyAlertResult(alerted, alerted ? "Speed" : null));
    }
}

public class VerifyAlertCommandLicenseStatusProcessor : ICommandProcessor<VerifyAlertCommand, VerifyAlertResult>
{
    public Task<VerifyAlertResult> ProcessAsync(VerifyAlertCommand command, CancellationToken cancellationToken = default)
    {
        var alerted = command.Metadata.IsLicenseSuspended;
        return Task.FromResult(new VerifyAlertResult(alerted, alerted ? "License" : null));
    }
}

public class VerifyAlertCommandMaintenanceProcessor : ICommandProcessor<VerifyAlertCommand, VerifyAlertResult>
{
    public Task<VerifyAlertResult> ProcessAsync(VerifyAlertCommand command, CancellationToken cancellationToken = default)
    {
        var alerted = command.Metadata.NeedsMaintenance;
        return Task.FromResult(new VerifyAlertResult(alerted, alerted ? "Maintenance" : null));
    }
}

// Handler that injects arrays of validators and processors
public class VerifyAlertCommandHandler(
    ICommandProcessor<VerifyAlertCommand, VerifyAlertResult>[] processors,
    ICommandValidator<VerifyAlertCommand, VerifyAlertResult>[] validators)
    : ICommandHandler<VerifyAlertCommand, VerifyAlertResult>
{
    public async Task<VerifyAlertResult> HandleAsync(VerifyAlertCommand command, CancellationToken cancellationToken = default)
    {
        // Run validators first; short-circuit on first alert
        foreach (var v in validators)
        {
            var res = await v.ValidateAsync(command, cancellationToken).ConfigureAwait(false);
            if (res.IsAlerted) return res;
        }

        // Run processors; stop on first alert
        foreach (var p in processors)
        {
            var res = await p.ProcessAsync(command, cancellationToken).ConfigureAwait(false);
            if (res.IsAlerted) return res;
        }

        return new VerifyAlertResult(false, null);
    }
}
