using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;

namespace Softalleys.Utilities.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="NavigationManager"/> class.
/// </summary>
public static class NavigationManagerExtensions
{
    /// <summary>
    /// Retrieves the value of a query parameter from the current URI.
    /// </summary>
    /// <param name="navigationManager">The <see cref="NavigationManager"/> instance.</param>
    /// <param name="key">The key of the query parameter to retrieve.</param>
    /// <returns>The value of the query parameter if found; otherwise, null.</returns>
    public static string? GetQueryValue(this NavigationManager navigationManager, string key)
    {
        var uri = navigationManager.ToAbsoluteUri(navigationManager.Uri);
        if (QueryHelpers.ParseQuery(uri.Query).TryGetValue(key, out var value)) return value;
        return null;
    }
}