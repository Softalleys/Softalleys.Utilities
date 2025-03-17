using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Softalleys.Utilities.Binders.OData;
using Softalleys.Utilities.Dependencies.Features.Authentication;
using Softalleys.Utilities.Formatters.OData;
using Softalleys.Utilities.Formatters.OData.Csv;

namespace Softalleys.Utilities;

/// <summary>
/// Provides extension methods for dependency injection configuration for Softalleys.Utilities.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Adds authentication utilities to the specified <see cref="IServiceCollection"/>.
    /// Registers the <see cref="IAuthSessionService"/> with its implementation <see cref="AuthSessionService"/> as a scoped service.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the authentication utilities to.</param>
    /// <returns>The <see cref="IServiceCollection"/> with the authentication utilities registered.</returns>
    public static IServiceCollection AddAuthenticationUtilities(this IServiceCollection services)
    {
        services.AddScoped<IAuthSessionService, AuthSessionService>();

        return services;
    }

    /// <summary>
    /// Adds the OData geospatial binder to the given <see cref="IServiceCollection"/>.
    /// Registers the <see cref="IFilterBinder"/> service with its implementation, <see cref="GeospatialFilterBinder"/>,
    /// as a scoped service.
    /// </summary>
    /// <param name="services">The service collection to which the geospatial binder will be added.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddODataGeospatialBinder(this IServiceCollection services)
    {
        services.AddScoped<IFilterBinder, GeospatialFilterBinder>();

        return services;
    }

    /// <summary>
    /// Adds the OData CSV media type resolvers to the given <see cref="IServiceCollection"/>.
    /// Registers both the default CSV media type resolver and a customized one as singleton services.
    /// </summary>
    /// <param name="services">The service collection to which the CSV media type resolvers will be added.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddODataCsvMediaTypeResolver(this IServiceCollection services)
    {
        services.AddSingleton<ODataMediaTypeResolver>(sp => CsvMediaTypeResolver.Instance);
        services.AddSingleton<ODataMediaTypeResolver>(sp => new CustomizedMediaTypeResolver());

        return services;
    }
}