using System.Text.Json.Nodes;

namespace Softalleys.Utilities.Extensions;

/// <summary>
///     Provides extension methods for working with JSON objects.
/// </summary>
public static class JsonObjectExtensions
{
    /// <summary>
    ///     Gets the value of a property from a JSON object.
    /// </summary>
    /// <typeparam name="T">The type of the value to retrieve.</typeparam>
    /// <param name="json">The JSON object to retrieve the property from.</param>
    /// <param name="name">The name of the property to retrieve.</param>
    /// <returns>The value of the property if it exists and is not null; otherwise, the default value of type T.</returns>
    public static T? GetProperty<T>(this JsonObject json, string name)
    {
        if (json.TryGetPropertyValue(name, out var value) && value != null) return value.GetValue<T>();
        return default;
    }

    /// <summary>
    ///     Sets the value of a property in a JSON object.
    /// </summary>
    /// <param name="json">The JSON object to set the property in.</param>
    /// <param name="name">The name of the property to set.</param>
    /// <param name="value">The value to set for the property. If null, the property is removed.</param>
    /// <returns>The JSON object with the property set.</returns>
    public static JsonObject SetProperty(this JsonObject json, string name, JsonNode? value)
    {
        if (value == null)
            json.Remove(name);
        else
            json[name] = value;
        return json;
    }
}