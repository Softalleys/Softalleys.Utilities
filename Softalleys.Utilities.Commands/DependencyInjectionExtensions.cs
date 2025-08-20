using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Softalleys.Utilities.Commands;

public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Adds Softalleys Commands services and scans assemblies for command handlers, validators, processors, and post-actions.
    /// </summary>
    public static IServiceCollection AddSoftalleysCommands(this IServiceCollection services, params Assembly[] assemblies)
    {
        if (assemblies == null || assemblies.Length == 0)
        {
            assemblies = [Assembly.GetCallingAssembly()];
        }

    // Mediator is scoped (safer with scoped dependencies like DbContext)
    services.TryAddScoped<ICommandMediator, CommandMediator>();

    // Handler invoker cache: compiled delegate cache used to avoid repeated reflection at runtime
    services.TryAddSingleton<IHandlerInvokerCache, HandlerInvokerCache>();

        // Expose the default handler as an open generic for DI construction if needed
        services.TryAddTransient(typeof(DefaultCommandHandler<,>));

        foreach (var assembly in assemblies)
        {
            RegisterCommandComponents(services, assembly);
        }

        return services;
    }

    private static void RegisterCommandComponents(IServiceCollection services, Assembly assembly)
    {
        var types = GetTypesFromAssembly(assembly);

        foreach (var type in types)
        {
            if (type.IsAbstract || type.IsInterface) continue;

            var ifaces = type.GetInterfaces().Where(i => i.IsGenericType).ToList();

            // Singleton command handlers via marker
            foreach (var singletonIface in ifaces.Where(i => i.GetGenericTypeDefinition() == typeof(ICommandSingletonHandler<,>)))
            {
                services.Add(new ServiceDescriptor(singletonIface, type, ServiceLifetime.Singleton));
                var args = singletonIface.GetGenericArguments();
                var baseIface = typeof(ICommandHandler<,>).MakeGenericType(args);
                services.Add(new ServiceDescriptor(baseIface, type, ServiceLifetime.Singleton));
            }

            // Scoped (default) command handlers
            foreach (var handlerIface in ifaces.Where(i => i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>)))
            {
                var args = handlerIface.GetGenericArguments();
                if (ifaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommandSingletonHandler<,>) && i.GenericTypeArguments.SequenceEqual(args)))
                    continue;
                services.Add(new ServiceDescriptor(handlerIface, type, ServiceLifetime.Scoped));
            }

            // Validators and processors are scoped by default
            foreach (var validatorIface in ifaces.Where(i => i.GetGenericTypeDefinition() == typeof(ICommandValidator<,>)))
            {
                services.Add(new ServiceDescriptor(validatorIface, type, ServiceLifetime.Scoped));
            }

            foreach (var processorIface in ifaces.Where(i => i.GetGenericTypeDefinition() == typeof(ICommandProcessor<,>)))
            {
                services.Add(new ServiceDescriptor(processorIface, type, ServiceLifetime.Scoped));
            }

            foreach (var postActionIface in ifaces.Where(i => i.GetGenericTypeDefinition() == typeof(ICommandPostAction<,>)))
            {
                services.Add(new ServiceDescriptor(postActionIface, type, ServiceLifetime.Scoped));
            }
        }
    }

    private static Type[] GetTypesFromAssembly(Assembly assembly)
    {
        try { return assembly.GetTypes(); }
        catch (ReflectionTypeLoadException ex) { return ex.Types.Where(t => t != null).ToArray()!; }
    }
}
