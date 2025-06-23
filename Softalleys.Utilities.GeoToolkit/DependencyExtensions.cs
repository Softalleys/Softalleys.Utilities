using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Softalleys.Utilities.GeoToolkit.Configuration;
using Softalleys.Utilities.GeoToolkit.Interfaces;
using Softalleys.Utilities.GeoToolkit.Providers;

namespace Softalleys.Utilities.GeoToolkit;

public static class DependencyExtensions
{
    public static IServiceCollection AddNominatimGeoToolkit(
        this IServiceCollection services,
        Action<GeoToolkitNominatimOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.ConfigureOptions(configureOptions);
        RegisterServices(services);

        return services;
    }

    public static IServiceCollection AddNominatimGeoToolkit(
        this IServiceCollection services,
        IConfigurationSection configurationSection)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.Configure<GeoToolkitNominatimOptions>(configurationSection);
        RegisterServices(services);

        return services;
    }

    public static IServiceCollection AddNominatimGeoToolkit(
        this IServiceCollection services,
        IConfiguration configuration,
        string name)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.Configure<GeoToolkitNominatimOptions>(configuration.GetSection(name));
        RegisterServices(services);

        return services;
    }

    private static void RegisterServices(IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddScoped<IGeocodingService, NominatimGeocodingService>();
        services.AddScoped<INominatimGeocodingService, NominatimGeocodingService>();
    }
}