using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Softalleys.Utilities.Whatsapp.Options;
using Softalleys.Utilities.Whatsapp.Services;

namespace Softalleys.Utilities.Whatsapp;

/// <summary>
/// Provides extension methods for registering WhatsApp message services and configuration in the dependency injection container.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Registers the WhatsApp message service and configures options using the provided delegate.
    /// </summary>
    /// <param name="services">The service collection to add the service to.</param>
    /// <param name="configureOptions">An action to configure <see cref="WhatsappBusinessOptions"/>.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddWhatsappMessageService(
        this IServiceCollection services,
        Action<WhatsappBusinessOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.Configure(configureOptions);
        services.AddSingleton<IWhatsappMessageService, WhatsappBusinessMessageService>();
        services.AddWhatsappHttpClient();

        return services;
    }

    /// <summary>
    /// Registers the WhatsApp message service and configures options from a named configuration section.
    /// </summary>
    /// <param name="services">The service collection to add the service to.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="name">The name of the configuration section.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="name"/> is null or whitespace.</exception>
    public static IServiceCollection AddWhatsappMessageService(
        this IServiceCollection services,
        IConfiguration configuration,
        string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Options name cannot be null or whitespace.", nameof(name));
        }

        services.Configure<WhatsappBusinessOptions>(configuration.GetSection(name));

        services.AddSingleton<IWhatsappMessageService, WhatsappBusinessMessageService>();
        services.AddWhatsappHttpClient();

        return services;
    }

    /// <summary>
    /// Registers the WhatsApp message service and configures options from the "WhatsappBusiness" configuration section.
    /// </summary>
    /// <param name="services">The service collection to add the service to.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddWhatsappMessageService(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<WhatsappBusinessOptions>(configuration.GetSection("WhatsappBusiness"));

        services.AddSingleton<IWhatsappMessageService, WhatsappBusinessMessageService>();
        services.AddWhatsappHttpClient();

        return services;
    }

    /// <summary>
    /// Registers the WhatsApp message service and configures options from the specified configuration section.
    /// </summary>
    /// <param name="services">The service collection to add the service to.</param>
    /// <param name="section">The configuration section containing WhatsApp business options.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddWhatsappMessageService(
        this IServiceCollection services,
        IConfigurationSection section)
    {
        ArgumentNullException.ThrowIfNull(section);

        services.Configure<WhatsappBusinessOptions>(section);

        services.AddSingleton<IWhatsappMessageService, WhatsappBusinessMessageService>();
        services.AddWhatsappHttpClient();

        return services;
    }

    private static void AddWhatsappHttpClient(this IServiceCollection services)
    {
        services.AddHttpClient("WhatsappBusinessApi")
            .ConfigureHttpClient((serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<WhatsappBusinessOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {options.Token}");
            });
    }
}