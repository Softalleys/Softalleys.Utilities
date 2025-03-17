using Microsoft.Extensions.DependencyInjection;
using Softalleys.Utilities.Dependencies.Features.Authentication;

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
}