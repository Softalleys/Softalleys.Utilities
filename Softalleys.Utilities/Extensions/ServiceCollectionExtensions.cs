using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Softalleys.Utilities.Extensions;

/// <summary>
///     Provides extension methods for <see cref="IServiceCollection" /> to enhance dependency injection capabilities.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Registers an alias for a service type to a specific implementation.
    /// </summary>
    /// <typeparam name="TService">The service type to be aliased.</typeparam>
    /// <typeparam name="TImplementation">The implementation type to use for the alias.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add the service to.</param>
    /// <returns>The updated <see cref="IServiceCollection" />.</returns>
    public static IServiceCollection AddAlias<TService, TImplementation>(this IServiceCollection services)
        where TImplementation : class, TService
        where TService : class
    {
        var descriptor = new ServiceDescriptor(
            typeof(TService),
            sp => sp.GetRequiredService<TImplementation>(),
            services.GetDescriptor<TImplementation>().Lifetime);

        services.Add(descriptor);
        return services;
    }

    /// <summary>
    ///     Composes a service type with multiple implementations into a single composite service.
    /// </summary>
    /// <typeparam name="TInterface">The interface type to be composed.</typeparam>
    /// <typeparam name="TComposite">The composite implementation type.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add the service to.</param>
    /// <param name="dependencies">The dependencies required by the composite service.</param>
    /// <returns>The updated <see cref="IServiceCollection" />.</returns>
    public static IServiceCollection Compose<TInterface, TComposite>(
        this IServiceCollection services,
        params Dependency[] dependencies)
        where TInterface : class where TComposite : class, TInterface
    {
        var parameterType = typeof(TComposite)
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public)
            .SelectMany(constructor => constructor.GetParameters(), (_, parameterInfo) => parameterInfo.ParameterType)
            .FirstOrDefault(type => type.IsAssignableFrom(typeof(TInterface[])));

        if (parameterType == null)
            throw new InvalidOperationException(
                $"The type {typeof(TComposite).FullName} has no public constructor that accepts {typeof(TInterface).FullName}[]");

        var serviceDescriptors = services
            .Where(descriptor => descriptor.ServiceType == typeof(TInterface))
            .ToArray();

        if (serviceDescriptors.Length <= 1)
            return services;

        // choose the shortest lifetime among existing service registrations
        var lifetime = serviceDescriptors.Max(descriptor => descriptor.Lifetime);

        var compositeDescriptor = ServiceDescriptor.Describe(
            typeof(TInterface),
            serviceProvider =>
            {
                var serviceInstances = Array.ConvertAll(
                    serviceDescriptors,
                    serviceDescriptor => (TInterface)serviceProvider.CreateService(serviceDescriptor));

                var serviceDependencies = Dependency.Override(parameterType, serviceInstances);
                return serviceProvider.CreateService<TComposite>(dependencies.Append(serviceDependencies));
            },
            lifetime);

        services.RemoveAll<TInterface>();
        services.Add(compositeDescriptor);

        return services;
    }

    /// <summary>
    ///     Automatically discovers and registers all implementations of the specified interface from assemblies,
    ///     then composes them into a single composite service. The composite must accept an array of the interface type.
    /// </summary>
    /// <typeparam name="TInterface">The interface type to find implementations for and compose.</typeparam>
    /// <typeparam name="TComposite">The composite implementation type that accepts TInterface[].</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add the services to.</param>
    /// <param name="assemblies">The assemblies to search for implementations.</param>
    /// <returns>The updated <see cref="IServiceCollection" />.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the composite type doesn't have a suitable constructor.</exception>
    public static IServiceCollection ComposeFromAssembly<TInterface, TComposite>(
        this IServiceCollection services,
        params Assembly[] assemblies)
        where TInterface : class
        where TComposite : class, TInterface
    {
        // Validate that TComposite has a constructor accepting TInterface[]
        var parameterType = typeof(TComposite)
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public)
            .SelectMany(constructor => constructor.GetParameters(), (_, parameterInfo) => parameterInfo.ParameterType)
            .FirstOrDefault(type => type.IsAssignableFrom(typeof(TInterface[])));

        if (parameterType == null)
            throw new InvalidOperationException(
                $"The type {typeof(TComposite).FullName} has no public constructor that accepts {typeof(TInterface).FullName}[]");

        // Discover and register all implementations of TInterface from the specified assemblies
        var implementations = FindAllImplementations<TInterface>(assemblies);

        foreach (var implementation in implementations.Where(impl => impl != typeof(TComposite)))
        {
            // Register each implementation as a scoped or singleton service depending on its lifetime
            var lifetime = DetermineServiceLifetime(implementation, services);
            var descriptor = new ServiceDescriptor(typeof(TInterface), implementation, lifetime);
            services.Add(descriptor);
        }

        // Compose the registered implementations into a composite service
        Compose<TInterface, TComposite>(services);

        return services;
    }

    /// <summary>
    ///     Decorates a registered service with a decorator implementation.
    /// </summary>
    /// <typeparam name="TInterface">The service type to be decorated.</typeparam>
    /// <typeparam name="TDecorator">The decorator implementation type.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add the service to.</param>
    /// <param name="dependencies">The dependencies required by the decorator.</param>
    /// <returns>The updated <see cref="IServiceCollection" />.</returns>
    public static IServiceCollection Decorate<TInterface, TDecorator>(
        this IServiceCollection services,
        params Dependency[] dependencies)
        where TInterface : class where TDecorator : class, TInterface
    {
        var serviceDescriptor = services.GetDescriptor<TInterface>();

        var decoratorDescriptor = ServiceDescriptor.Describe(
            typeof(TInterface),
            serviceProvider =>
            {
                var instance = Dependency.Override((TInterface)serviceProvider.CreateService(serviceDescriptor));
                return serviceProvider.CreateService<TDecorator>(dependencies.Append(instance));
            },
            serviceDescriptor.Lifetime);

        return services.Replace(decoratorDescriptor);
    }

    /// <summary>
    ///     Appends an element to an array.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <param name="source">The source array.</param>
    /// <param name="element">The element to append.</param>
    /// <returns>A new array with the appended element.</returns>
    private static T[] Append<T>(this T[] source, T element)
    {
        switch (source)
        {
            case { Length: > 0 }:
                Array.Resize(ref source, source.Length + 1);
                source[^1] = element;
                return source;

            default:
                return new[] { element };
        }
    }

    /// <summary>
    ///     Gets the service descriptor for a specified service type.
    /// </summary>
    /// <typeparam name="TInterface">The service type.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to search.</param>
    /// <returns>The service descriptor for the specified service type.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the service type is not registered.</exception>
    private static ServiceDescriptor GetDescriptor<TInterface>(this IServiceCollection services)
        where TInterface : class
    {
        return services.SingleOrDefault(s => s.ServiceType == typeof(TInterface))
               ?? throw new InvalidOperationException($"{typeof(TInterface).Name} is not registered");
    }

    /// <summary>
    ///     Creates a service instance from a service descriptor.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="descriptor">The service descriptor.</param>
    /// <returns>The created service instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the service instance cannot be created.</exception>
    private static object CreateService(this IServiceProvider serviceProvider, ServiceDescriptor descriptor)
    {
        return descriptor switch
        {
            { ImplementationInstance: { } instance } => instance,
            { ImplementationFactory: { } factory } => factory(serviceProvider),
            { ImplementationType: { } type } => ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, type),
            _ => throw new InvalidOperationException($"Unable to create instance of {descriptor.ServiceType.FullName}")
        };
    }

    /// <summary>
    ///     Creates a service instance of the specified type with custom dependencies.
    /// </summary>
    /// <typeparam name="T">The type of the service to create.</typeparam>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="dependencies">The dependencies required by the service.</param>
    /// <returns>The created service instance.</returns>
    public static T CreateService<T>(this IServiceProvider serviceProvider, params Dependency[] dependencies)
    {
        return (T)serviceProvider.CreateService(typeof(T), dependencies);
    }

    /// <summary>
    ///     Creates a service instance of the specified type with custom dependencies.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="type">The type of the service to create.</param>
    /// <param name="dependencies">The dependencies required by the service.</param>
    /// <returns>The created service instance.</returns>
    public static object CreateService(this IServiceProvider serviceProvider,
        Type type, params Dependency[] dependencies)
    {
        var factory = ActivatorUtilities.CreateFactory(type, Array.ConvertAll(dependencies, d => d.Type));
        return factory(serviceProvider, Array.ConvertAll(dependencies, d => d.Factory(serviceProvider)));
    }

    /// <summary>
    ///     Registers a transient service with custom dependencies.
    /// </summary>
    /// <typeparam name="T">The service type to register.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add the service to.</param>
    /// <param name="dependencies">The dependencies required by the service.</param>
    /// <returns>The updated <see cref="IServiceCollection" />.</returns>
    public static IServiceCollection AddTransient<T>(this IServiceCollection services, params Dependency[] dependencies)
        where T : class
    {
        return services.AddTransient(sp => sp.CreateService<T>(dependencies));
    }

    /// <summary>
    ///     Registers a transient service with custom dependencies.
    /// </summary>
    /// <typeparam name="TService">The type of the service to add.</typeparam>
    /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add the service to.</param>
    /// <param name="dependencies">The dependencies required by the service.</param>
    /// <returns>The updated <see cref="IServiceCollection" />.</returns>
    public static IServiceCollection AddTransient<TService, TImplementation>(this IServiceCollection services,
        params Dependency[] dependencies)
        where TService : class
        where TImplementation : class, TService
    {
        return services.AddTransient<TService, TImplementation>(sp => sp.CreateService<TImplementation>(dependencies));
    }

    /// <summary>
    ///     Registers a scoped service of the type specified in <typeparamref name="T" /> with custom dependencies.
    ///     A scoped service is created once per request within the scope.
    /// </summary>
    /// <typeparam name="T">The type of the service to add.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add the service to.</param>
    /// <param name="dependencies">
    ///     An array of <see cref="Dependency" /> objects representing additional dependencies required
    ///     by the service.
    /// </param>
    /// <returns>The updated <see cref="IServiceCollection" />.</returns>
    public static IServiceCollection AddScoped<T>(this IServiceCollection services, params Dependency[] dependencies)
        where T : class
    {
        return services.AddScoped(sp => sp.CreateService<T>(dependencies));
    }

    /// <summary>
    ///     Registers a scoped service with the implementation type specified in <typeparamref name="TImplementation" />
    ///     and the service type specified in <typeparamref name="TService" /> with custom dependencies.
    ///     A scoped service is created once per request within the scope.
    /// </summary>
    /// <typeparam name="TService">The type of the service to add.</typeparam>
    /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add the service to.</param>
    /// <param name="dependencies">
    ///     An array of <see cref="Dependency" /> objects representing additional dependencies required
    ///     by the service.
    /// </param>
    /// <returns>The updated <see cref="IServiceCollection" />.</returns>
    public static IServiceCollection AddScoped<TService, TImplementation>(this IServiceCollection services,
        params Dependency[] dependencies)
        where TService : class
        where TImplementation : class, TService
    {
        return services.AddScoped<TService, TImplementation>(sp => sp.CreateService<TImplementation>(dependencies));
    }

    /// <summary>
    ///     Registers a singleton service of the type specified in <typeparamref name="T" /> with custom dependencies.
    ///     A singleton service is created the first time it is requested, and subsequent requests use the same instance.
    /// </summary>
    /// <typeparam name="T">The type of the service to add.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add the service to.</param>
    /// <param name="dependencies">
    ///     An array of <see cref="Dependency" /> objects representing additional dependencies required
    ///     by the service.
    /// </param>
    /// <returns>The updated <see cref="IServiceCollection" />.</returns>
    public static IServiceCollection AddSingleton<T>(this IServiceCollection services, params Dependency[] dependencies)
        where T : class
    {
        return services.AddSingleton(sp => sp.CreateService<T>(dependencies));
    }

    /// <summary>
    ///     Registers a singleton service of the type specified in <typeparamref name="TService" /> with custom dependencies.
    ///     A singleton service is created the first time it is requested, and subsequent requests use the same instance.
    /// </summary>
    /// <typeparam name="TService">The type of the service to add.</typeparam>
    /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add the service to.</param>
    /// <param name="dependencies">
    ///     An array of <see cref="Dependency" /> objects representing additional dependencies required
    ///     by the service.
    /// </param>
    /// <returns>The updated <see cref="IServiceCollection" />.</returns>
    public static IServiceCollection AddSingleton<TService, TImplementation>(this IServiceCollection services,
        params Dependency[] dependencies)
        where TService : class
        where TImplementation : class, TService
    {
        return services.AddSingleton<TService, TImplementation>(sp => sp.CreateService<TImplementation>(dependencies));
    }

    /// <summary>
    ///     Automatically registers a scoped service by finding implementations of the specified interface in the provided assemblies.
    /// </summary>
    /// <typeparam name="TService">The service interface type to find implementations for.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add the service to.</param>
    /// <param name="assemblies">The assemblies to search for implementations.</param>
    /// <returns>The updated <see cref="IServiceCollection" />.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no implementation or multiple implementations are found.</exception>
    public static IServiceCollection AddScopedFromAssembly<TService>(this IServiceCollection services,
        params Assembly[] assemblies)
        where TService : class
    {
        var implementation = FindSingleImplementation<TService>(assemblies);
        services.AddScoped(typeof(TService), implementation);
        return services;
    }

    /// <summary>
    ///     Automatically registers a transient service by finding implementations of the specified interface in the provided assemblies.
    /// </summary>
    /// <typeparam name="TService">The service interface type to find implementations for.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add the service to.</param>
    /// <param name="assemblies">The assemblies to search for implementations.</param>
    /// <returns>The updated <see cref="IServiceCollection" />.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no implementation or multiple implementations are found.</exception>
    public static IServiceCollection AddTransientFromAssembly<TService>(this IServiceCollection services,
        params Assembly[] assemblies)
        where TService : class
    {
        var implementation = FindSingleImplementation<TService>(assemblies);
        services.AddTransient(typeof(TService), implementation);
        return services;
    }

    /// <summary>
    ///     Automatically registers a singleton service by finding implementations of the specified interface in the provided assemblies.
    /// </summary>
    /// <typeparam name="TService">The service interface type to find implementations for.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add the service to.</param>
    /// <param name="assemblies">The assemblies to search for implementations.</param>
    /// <returns>The updated <see cref="IServiceCollection" />.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no implementation or multiple implementations are found.</exception>
    public static IServiceCollection AddSingletonFromAssembly<TService>(this IServiceCollection services,
        params Assembly[] assemblies)
        where TService : class
    {
        var implementation = FindSingleImplementation<TService>(assemblies);
        services.AddSingleton(typeof(TService), implementation);
        return services;
    }

    /// <summary>
    ///     Automatically registers a service by finding implementations of the specified interface in the provided assemblies.
    ///     The service lifetime is determined by analyzing the dependencies of the implementation:
    ///     - Singleton if the implementation or its dependencies suggest singleton lifetime
    ///     - Scoped otherwise
    /// </summary>
    /// <typeparam name="TService">The service interface type to find implementations for.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add the service to.</param>
    /// <param name="assemblies">The assemblies to search for implementations.</param>
    /// <returns>The updated <see cref="IServiceCollection" />.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no implementation or multiple implementations are found.</exception>
    public static IServiceCollection AddServiceFromAssembly<TService>(this IServiceCollection services,
        params Assembly[] assemblies)
        where TService : class
    {
        var implementation = FindSingleImplementation<TService>(assemblies);
        var lifetime = DetermineServiceLifetime(implementation, services);
        
        var descriptor = new ServiceDescriptor(typeof(TService), implementation, lifetime);
        services.Add(descriptor);
        
        return services;
    }




    /// <summary>
    ///     Finds a single implementation of the specified service interface in the provided assemblies.
    /// </summary>
    /// <typeparam name="TService">The service interface type to find implementations for.</typeparam>
    /// <param name="assemblies">The assemblies to search for implementations.</param>
    /// <returns>The implementation type.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no implementation or multiple implementations are found.</exception>
    private static Type FindSingleImplementation<TService>(Assembly[] assemblies)
        where TService : class
    {
        var serviceType = typeof(TService);
        
        var implementations = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsClass && 
                          !type.IsAbstract && 
                          serviceType.IsAssignableFrom(type) &&
                          type != serviceType) // Exclude the interface itself
            .ToArray();

        return implementations.Length switch
        {
            0 => throw new InvalidOperationException(
                $"No implementation found for {serviceType.Name} in the provided assemblies: {string.Join(", ", assemblies.Select(a => a.GetName().Name))}"),
            1 => implementations[0],
            _ => throw new InvalidOperationException(
                $"Multiple implementations found for {serviceType.Name}: {string.Join(", ", implementations.Select(t => t.Name))}. " +
                "Please ensure only one implementation exists or use specific registration methods.")
        };
    }
    
    /// <summary>
    ///     Finds all implementations of the specified service interface in the provided assemblies.
    /// </summary>
    /// <typeparam name="TService">The service interface type to find implementations for.</typeparam>
    /// <param name="assemblies">The assemblies to search for implementations.</param>
    /// <returns>An array of implementation types.</returns>
    private static Type[] FindAllImplementations<TService>(Assembly[] assemblies)
        where TService : class
    {
        var serviceType = typeof(TService);

        return assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsClass &&
                          !type.IsAbstract &&
                          serviceType.IsAssignableFrom(type) &&
                          type != serviceType) // Exclude the interface itself
            .ToArray();
    }

    /// <summary>
    ///     Determines the appropriate service lifetime based on the implementation type and its dependencies.
    /// </summary>
    /// <param name="implementationType">The implementation type to analyze.</param>
    /// <param name="services">The service collection to check for existing registrations.</param>
    /// <returns>The recommended service lifetime.</returns>
    private static ServiceLifetime DetermineServiceLifetime(Type implementationType, IServiceCollection services)
    {
        // Check if the implementation has any attributes that suggest lifetime
        var lifetimeAttributes = implementationType.GetCustomAttributes(typeof(Attribute), true)
            .Select(attr => attr.GetType().Name.ToLowerInvariant())
            .ToArray();

        // Look for common lifetime indicators in attribute names
        if (lifetimeAttributes.Any(attr => attr.Contains("singleton") || attr.Contains("single")))
            return ServiceLifetime.Singleton;

        if (lifetimeAttributes.Any(attr => attr.Contains("transient")))
            return ServiceLifetime.Transient;

        if (lifetimeAttributes.Any(attr => attr.Contains("scoped")))
            return ServiceLifetime.Scoped;

        // Analyze constructor dependencies
        var constructors = implementationType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
        var primaryConstructor = constructors
            .OrderByDescending(c => c.GetParameters().Length)
            .FirstOrDefault();

        if (primaryConstructor == null)
            return ServiceLifetime.Scoped; // Default to scoped

        var parameterTypes = primaryConstructor.GetParameters().Select(p => p.ParameterType).ToArray();
        
        // Check if any dependencies are registered as singleton
        var hasSingletonDependencies = parameterTypes.Any(paramType =>
            services.Any(descriptor => 
                descriptor.ServiceType == paramType && 
                descriptor.Lifetime == ServiceLifetime.Singleton));

        // Check for stateless patterns (no fields, or only readonly fields)
        var hasState = implementationType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .Any(field => !field.IsInitOnly && !field.IsLiteral);

        // Decision logic:
        // 1. If it has singleton dependencies and appears stateless, make it singleton
        // 2. If it has mutable state, make it scoped (safer default)
        // 3. Otherwise, default to scoped
        
        if (hasSingletonDependencies && !hasState)
            return ServiceLifetime.Singleton;

        return ServiceLifetime.Scoped; // Safe default
    }
}