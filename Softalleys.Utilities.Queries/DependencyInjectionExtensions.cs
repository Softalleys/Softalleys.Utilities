using System.Reflection;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Softalleys.Utilities.Queries;

public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Adds Softalleys Queries services and scans assemblies for query handlers.
    /// </summary>
    public static IServiceCollection AddSoftalleysQueries(this IServiceCollection services, params Assembly[] assemblies)
    {
        if (assemblies == null || assemblies.Length == 0)
        {
            assemblies = [Assembly.GetCallingAssembly()];
        }

        // Dispatcher is singleton as requested.
        services.TryAddSingleton<IQueryDispatcher, QueryDispatcher>();
        
        // Register IQueryMediator as an alias for IQueryDispatcher with SendAsync methods
        services.TryAddSingleton<IQueryMediator, QueryMediator>();

        foreach (var assembly in assemblies)
        {
            RegisterQueryHandlers(services, assembly);
        }

        return services;
    }

    private static void RegisterQueryHandlers(IServiceCollection services, Assembly assembly)
    {
        var types = GetTypesFromAssembly(assembly);

        foreach (var type in types)
        {
            if (type.IsAbstract || type.IsInterface) continue;

            var interfaces = type.GetInterfaces().Where(i => i.IsGenericType).ToList();

            // Handle singleton marker for regular queries
            var singletonQueryIfaces = interfaces
                .Where(i => i.GetGenericTypeDefinition() == typeof(IQuerySingletonHandler<,>))
                .ToList();

            foreach (var singletonIface in singletonQueryIfaces)
            {
                // Register the marker interface itself (optional but useful for IEnumerable resolves)
                services.Add(new ServiceDescriptor(singletonIface, type, ServiceLifetime.Singleton));

                // Also register the base IQueryHandler<TQuery,TResponse> as Singleton so dispatcher can resolve it
                var args = singletonIface.GetGenericArguments();
                var baseIface = typeof(IQueryHandler<,>).MakeGenericType(args);
                services.Add(new ServiceDescriptor(baseIface, type, ServiceLifetime.Singleton));
            }

            // Handle non-singleton regular query handlers (scoped)
            var regularQueryIfaces = interfaces
                .Where(i => i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>))
                .ToList();
            foreach (var iface in regularQueryIfaces)
            {
                // Skip if this type is also registered as singleton for the same generic args
                var args = iface.GetGenericArguments();
                var correspondingSingleton = typeof(IQuerySingletonHandler<,>).MakeGenericType(args);
                if (interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQuerySingletonHandler<,>) && i.GenericTypeArguments.SequenceEqual(args)))
                    continue;

                services.Add(new ServiceDescriptor(iface, type, ServiceLifetime.Scoped));
            }

            // Handle singleton marker for stream queries
            var singletonStreamIfaces = interfaces
                .Where(i => i.GetGenericTypeDefinition() == typeof(IQueryStreamSingletonHandler<,>))
                .ToList();
            foreach (var singletonIface in singletonStreamIfaces)
            {
                services.Add(new ServiceDescriptor(singletonIface, type, ServiceLifetime.Singleton));
                var args = singletonIface.GetGenericArguments();
                var baseIface = typeof(IQueryStreamHandler<,>).MakeGenericType(args);
                services.Add(new ServiceDescriptor(baseIface, type, ServiceLifetime.Singleton));
            }

            // Handle non-singleton stream query handlers (scoped)
            var regularStreamIfaces = interfaces
                .Where(i => i.GetGenericTypeDefinition() == typeof(IQueryStreamHandler<,>))
                .ToList();
            foreach (var iface in regularStreamIfaces)
            {
                var args = iface.GetGenericArguments();
                var correspondingSingleton = typeof(IQueryStreamSingletonHandler<,>).MakeGenericType(args);
                if (interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryStreamSingletonHandler<,>) && i.GenericTypeArguments.SequenceEqual(args)))
                    continue;

                services.Add(new ServiceDescriptor(iface, type, ServiceLifetime.Scoped));
            }
        }
    }

    private static Type[] GetTypesFromAssembly(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t != null).ToArray()!;
        }
    }

    private static void RegisterHandlersOfType(IServiceCollection services, Type[] types, Type handlerInterfaceType, ServiceLifetime lifetime)
    {
        foreach (var type in types)
        {
            if (type.IsAbstract || type.IsInterface) continue;

            var handlerInterfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterfaceType)
                .ToList();

            foreach (var handlerInterface in handlerInterfaces)
            {
                services.Add(new ServiceDescriptor(handlerInterface, type, lifetime));
            }
        }
    }
}
