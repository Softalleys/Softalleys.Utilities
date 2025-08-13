using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Softalleys.Utilities.Events;

/// <summary>
/// Provides extension methods for dependency injection configuration for Softalleys.Utilities.Events.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Adds Softalleys Events services to the specified <see cref="IServiceCollection"/>.
    /// Automatically scans the provided assemblies for event handlers and registers them with appropriate lifetimes.
    /// If no assemblies are provided, scans the calling assembly.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="assemblies">The assemblies to scan for event handlers. If empty, uses the calling assembly.</param>
    /// <returns>The <see cref="IServiceCollection"/> with the event services registered.</returns>
    /// <example>
    /// <code>
    /// // Register handlers from current assembly
    /// services.AddSoftalleysEvents();
    /// 
    /// // Register handlers from specific assemblies
    /// services.AddSoftalleysEvents(typeof(MyEventHandler).Assembly, typeof(AnotherHandler).Assembly);
    /// </code>
    /// </example>
    public static IServiceCollection AddSoftalleysEvents(this IServiceCollection services, params Assembly[] assemblies)
    {
        if (assemblies == null || assemblies.Length == 0)
        {
            // Default to calling assembly if none provided
            assemblies = new[] { Assembly.GetCallingAssembly() };
        }

        // Register the EventBus as scoped service
        services.TryAddScoped<IEventBus, EventBus>();

        // Scan each assembly for handlers and register them
        foreach (var assembly in assemblies)
        {
            RegisterEventHandlers(services, assembly);
        }

        return services;
    }

    /// <summary>
    /// Registers all event handlers found in the specified assembly with their appropriate lifetimes.
    /// </summary>
    /// <param name="services">The service collection to register handlers with.</param>
    /// <param name="assembly">The assembly to scan for handlers.</param>
    private static void RegisterEventHandlers(IServiceCollection services, Assembly assembly)
    {
        var types = GetTypesFromAssembly(assembly);

        // Register scoped handlers
        RegisterHandlersOfType(services, types, typeof(IEventHandler<>), ServiceLifetime.Scoped);
        RegisterHandlersOfType(services, types, typeof(IEventPreHandler<>), ServiceLifetime.Scoped);
        RegisterHandlersOfType(services, types, typeof(IEventPostHandler<>), ServiceLifetime.Scoped);

        // Register singleton handlers
        RegisterHandlersOfType(services, types, typeof(IEventSingletonHandler<>), ServiceLifetime.Singleton);
        RegisterHandlersOfType(services, types, typeof(IEventPreSingletonHandler<>), ServiceLifetime.Singleton);
        RegisterHandlersOfType(services, types, typeof(IEventPostSingletonHandler<>), ServiceLifetime.Singleton);

        // Register hosted event handlers (singletons shared with IHostedService)
        RegisterHostedHandlers(services, types);
    }

    /// <summary>
    /// Gets all types from an assembly, handling any exceptions that might occur.
    /// </summary>
    /// <param name="assembly">The assembly to get types from.</param>
    /// <returns>An array of types found in the assembly.</returns>
    private static Type[] GetTypesFromAssembly(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            // Return only the types that loaded successfully
            return ex.Types.Where(t => t != null).ToArray()!;
        }
    }

    /// <summary>
    /// Registers all implementations of a generic handler interface with the specified lifetime.
    /// </summary>
    /// <param name="services">The service collection to register with.</param>
    /// <param name="types">The types to search through.</param>
    /// <param name="handlerInterfaceType">The generic handler interface type (e.g., typeof(IEventHandler&lt;&gt;)).</param>
    /// <param name="lifetime">The service lifetime to register handlers with.</param>
    private static void RegisterHandlersOfType(IServiceCollection services, Type[] types, Type handlerInterfaceType, ServiceLifetime lifetime)
    {
        foreach (var type in types)
        {
            // Skip abstract classes and interfaces
            if (type.IsAbstract || type.IsInterface)
                continue;

            // Find all interfaces this type implements that match our handler interface
            var handlerInterfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterfaceType)
                .ToList();

            // Register each handler interface this type implements
            foreach (var handlerInterface in handlerInterfaces)
            {
                var serviceDescriptor = new ServiceDescriptor(handlerInterface, type, lifetime);
                services.Add(serviceDescriptor);
            }
        }
    }

    /// <summary>
    /// Registers event hosted services so that a single singleton instance is shared between
    /// IEventHostedService<TEvent> and IHostedService.
    /// </summary>
    private static void RegisterHostedHandlers(IServiceCollection services, Type[] types)
    {
        // Ensure one IHostedService registration per concrete type even if it implements multiple events
        var registeredHostedTypes = new HashSet<Type>();

        foreach (var type in types)
        {
            if (type.IsAbstract || type.IsInterface)
                continue;

            var hostedInterfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHostedService<>))
                .ToList();

            if (hostedInterfaces.Count == 0)
                continue;

            // Register the concrete type as singleton
            services.TryAddSingleton(type);

            // Map each closed IEventHostedService<T> to the same singleton instance
            foreach (var hostedInterface in hostedInterfaces)
            {
                services.TryAddSingleton(hostedInterface, sp => sp.GetRequiredService(type));
            }

            // Register as IHostedService once per concrete type (enumerable)
            if (!registeredHostedTypes.Contains(type))
            {
                // Factory mapping ensures IHostedService resolves to the same singleton instance of 'type'
                services.AddSingleton(typeof(IHostedService), sp => (IHostedService)sp.GetRequiredService(type));
                registeredHostedTypes.Add(type);
            }
        }
    }
}
