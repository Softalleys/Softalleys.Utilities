using Microsoft.Extensions.DependencyInjection;

namespace Softalleys.Utilities.Commands.Tests;

public class ArrayInjectionHandlerTests
{
    // Simple result hierarchy with validation marker
    private abstract record RBase;
    private record RValid : RBase, IValidationStageResult { public bool Continue => true; }
    private record RFail(string Reason) : RBase, Interfaces.IErrorResponse
    {
        public string Error { get; init; } = "Bad Request";
        public string Title { get; init; } = "Validation Error";
        public string Message { get; init; } = Reason;
        public int Status { get; init; } = 400;
        public IDictionary<string, string[]> Errors { get; init; } = new Dictionary<string, string[]>();
        public string? TraceId { get; init; }
    }
    private record ROk(int Value) : RBase;

    private record C(int X) : ICommand<RBase>;

    private class NonNegativeValidator : ICommandValidator<C, RBase>
    {
        public Task<RBase> ValidateAsync(C command, CancellationToken cancellationToken = default)
            => command.X < 0
                ? Task.FromResult<RBase>(new RFail("X must be non-negative"))
                : Task.FromResult<RBase>(new RValid());
    }

    private class LessThanHundredValidator : ICommandValidator<C, RBase>
    {
        public Task<RBase> ValidateAsync(C command, CancellationToken cancellationToken = default)
            => command.X >= 100
                ? Task.FromResult<RBase>(new RFail("X must be < 100"))
                : Task.FromResult<RBase>(new RValid());
    }

    private class DoubleProcessor : ICommandProcessor<C, RBase>
    {
        public Task<RBase> ProcessAsync(C command, CancellationToken cancellationToken = default)
            => Task.FromResult<RBase>(new ROk(command.X * 2));
    }

    private class IncrementProcessor : ICommandProcessor<C, RBase>
    {
        public Task<RBase> ProcessAsync(C command, CancellationToken cancellationToken = default)
            => Task.FromResult<RBase>(new ROk(command.X + 1));
    }

    // Handler that injects arrays (T[]) directly
    private class ArrayBasedHandler(
        ICommandValidator<C, RBase>[] validators,
        ICommandProcessor<C, RBase>[] processors) : ICommandHandler<C, RBase>
    {
        public async Task<RBase> HandleAsync(C command, CancellationToken cancellationToken = default)
        {
            // Validate (short-circuit on first failure)
            foreach (var v in validators)
            {
                var res = await v.ValidateAsync(command, cancellationToken).ConfigureAwait(false);
                if (res is not IValidationStageResult ok || !ok.Continue)
                    return res;
            }

            // Process (last one wins)
            RBase result = default!;
            foreach (var p in processors)
            {
                result = await p.ProcessAsync(command, cancellationToken).ConfigureAwait(false);
            }
            return result;
        }
    }

    [Fact]
    public async Task Handler_With_Array_Injection_Receives_All_Services()
    {
        var services = new ServiceCollection();
        // Core services
        services.AddScoped<ICommandMediator, CommandMediator>();
        services.AddSingleton<IHandlerInvokerCache, HandlerInvokerCache>();

        // Register handler and components explicitly (avoid scanning both handlers)
        services.AddScoped<ICommandHandler<C, RBase>, ArrayBasedHandler>();
        services.AddScoped<ICommandValidator<C, RBase>, NonNegativeValidator>();
        services.AddScoped<ICommandValidator<C, RBase>, LessThanHundredValidator>();
        services.AddScoped<ICommandProcessor<C, RBase>, DoubleProcessor>();
        services.AddScoped<ICommandProcessor<C, RBase>, IncrementProcessor>();

        // Register array adapters for validators/processors
        services.AddTransient<ICommandValidator<C, RBase>[]>(sp => sp.GetServices<ICommandValidator<C, RBase>>().ToArray());
        services.AddTransient<ICommandProcessor<C, RBase>[]>(sp => sp.GetServices<ICommandProcessor<C, RBase>>().ToArray());

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<ICommandMediator>();

        // Both validators pass, processors run in order: Double then Increment (last wins)
        var ok = await mediator.SendAsync<RBase, C>(new C(10));
        var success = Assert.IsType<ROk>(ok);
        Assert.Equal(11, success.Value); // last processor (Increment) wins

        // First validator fails
        var fail1 = await mediator.SendAsync<RBase, C>(new C(-5));
        var f1 = Assert.IsType<RFail>(fail1);
        Assert.Equal("X must be non-negative", f1.Message);

        // Second validator fails
        var fail2 = await mediator.SendAsync<RBase, C>(new C(150));
        var f2 = Assert.IsType<RFail>(fail2);
        Assert.Equal("X must be < 100", f2.Message);
    }

    // Also verify single-item injection (only one validator/processor registered)
    private class SingleValidator : ICommandValidator<C, RBase>
    {
        public Task<RBase> ValidateAsync(C command, CancellationToken cancellationToken = default)
            => Task.FromResult<RBase>(new RValid());
    }
    private class SingleProcessor : ICommandProcessor<C, RBase>
    {
        public Task<RBase> ProcessAsync(C command, CancellationToken cancellationToken = default)
            => Task.FromResult<RBase>(new ROk(command.X * 3));
    }

    private class ArrayHandlerSingle(
        ICommandValidator<C, RBase>[] validators,
        ICommandProcessor<C, RBase>[] processors) : ICommandHandler<C, RBase>
    {
        public Task<RBase> HandleAsync(C command, CancellationToken cancellationToken = default)
            => processors[0].ProcessAsync(command, cancellationToken);
    }

    [Fact]
    public async Task Handler_With_Array_Injection_Works_With_Single_Items()
    {
        var services = new ServiceCollection();
        // Core services
        services.AddScoped<ICommandMediator, CommandMediator>();
        services.AddSingleton<IHandlerInvokerCache, HandlerInvokerCache>();

        // Register single validator/processor and the array-injecting handler
        services.AddScoped<ICommandHandler<C, RBase>, ArrayHandlerSingle>();
        services.AddScoped<ICommandValidator<C, RBase>, SingleValidator>();
        services.AddScoped<ICommandProcessor<C, RBase>, SingleProcessor>();

        // Array adapters
        services.AddTransient<ICommandValidator<C, RBase>[]>(sp => sp.GetServices<ICommandValidator<C, RBase>>().ToArray());
        services.AddTransient<ICommandProcessor<C, RBase>[]>(sp => sp.GetServices<ICommandProcessor<C, RBase>>().ToArray());

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<ICommandMediator>();

        var ok = await mediator.SendAsync<RBase, C>(new C(4));
        var success = Assert.IsType<ROk>(ok);
        Assert.Equal(12, success.Value);
    }
}
