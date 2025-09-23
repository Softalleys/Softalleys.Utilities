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
        // Track which array service types we've already registered to avoid duplicates
        var registeredArrayTypes = new HashSet<Type>();

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

                // If handler requests arrays like ICommandValidator<TCommand,TResult>[] or ICommandProcessor<TCommand,TResult>[]
                // ensure those array service types are resolvable (build from registered IEnumerable<T> services or empty).
                foreach (var ctor in type.GetConstructors())
                {
                    foreach (var param in ctor.GetParameters())
                    {
                        if (!param.ParameterType.IsArray) continue;
                        var elemType = param.ParameterType.GetElementType();
                        if (elemType == null || !elemType.IsGenericType) continue;
                        var genericDef = elemType.GetGenericTypeDefinition();
                        if (genericDef == typeof(ICommandValidator<,>) || genericDef == typeof(ICommandProcessor<,>) || genericDef == typeof(ICommandPostAction<,>))
                        {
                            var arrayType = param.ParameterType; // elemType[]
                            EnsureArrayRegistration(services, registeredArrayTypes, arrayType, elemType);
                        }
                    }
                }
            }

            // Validators and processors are scoped by default
            foreach (var validatorIface in ifaces.Where(i => i.GetGenericTypeDefinition() == typeof(ICommandValidator<,>)))
            {
                services.Add(new ServiceDescriptor(validatorIface, type, ServiceLifetime.Scoped));
                // Also provide T[] adapter for handlers that inject arrays
                var arrayType = validatorIface.MakeArrayType();
                EnsureArrayRegistration(services, registeredArrayTypes, arrayType, validatorIface);
            }

            foreach (var processorIface in ifaces.Where(i => i.GetGenericTypeDefinition() == typeof(ICommandProcessor<,>)))
            {
                services.Add(new ServiceDescriptor(processorIface, type, ServiceLifetime.Scoped));
                var arrayType = processorIface.MakeArrayType();
                EnsureArrayRegistration(services, registeredArrayTypes, arrayType, processorIface);
            }

            foreach (var postActionIface in ifaces.Where(i => i.GetGenericTypeDefinition() == typeof(ICommandPostAction<,>)))
            {
                services.Add(new ServiceDescriptor(postActionIface, type, ServiceLifetime.Scoped));
                var arrayType = postActionIface.MakeArrayType();
                EnsureArrayRegistration(services, registeredArrayTypes, arrayType, postActionIface);
            }
        }
    }

    private static void EnsureArrayRegistration(IServiceCollection services, HashSet<Type> registeredArrayTypes, Type arrayType, Type elementType)
    {
        if (registeredArrayTypes.Contains(arrayType)) return;
        // Avoid double registration if user called AddSoftalleysCommands multiple times
        if (services.Any(sd => sd.ServiceType == arrayType))
        {
            registeredArrayTypes.Add(arrayType);
            return;
        }

        // Register a transient factory that materializes the array from the resolved IEnumerable<elementType>
        services.AddTransient(arrayType, sp =>
        {
            // Resolve all elements; may be empty
            var objs = sp.GetServices(elementType).Cast<object>().ToList();
            var arr = Array.CreateInstance(elementType, objs.Count);
            for (int i = 0; i < objs.Count; i++)
            {
                arr.SetValue(objs[i], i);
            }
            return arr;
        });
        registeredArrayTypes.Add(arrayType);
    }

    private static Type[] GetTypesFromAssembly(Assembly assembly)
    {
        try { return assembly.GetTypes(); }
        catch (ReflectionTypeLoadException ex) { return ex.Types.Where(t => t != null).ToArray()!; }
    }
}
