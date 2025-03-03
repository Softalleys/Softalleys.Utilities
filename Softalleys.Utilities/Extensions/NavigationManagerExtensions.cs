using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;

namespace Softalleys.Utilities.Extensions;

public static class NavigationManagerExtensions
{
    public static string? GetQueryValue(this NavigationManager navigationManager, string key)
    {
        var uri = navigationManager.ToAbsoluteUri(navigationManager.Uri);
        if (QueryHelpers.ParseQuery(uri.Query).TryGetValue(key, out var value)) return value;
        return null;
    }
}