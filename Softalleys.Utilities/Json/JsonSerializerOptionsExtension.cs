using System.Text.Json;
using System.Text.Json.Serialization;

namespace Softalleys.Utilities.Json;

/// <summary>
///     Provides extension methods for configuring and using <see cref="JsonSerializerOptions" />.
/// </summary>
public static class JsonSerializerOptionsExtension
{
    /// <summary>
    ///     Adds default JSON serialization options.
    /// </summary>
    /// <param name="options">The <see cref="JsonSerializerOptions" /> to configure.</param>
    /// <returns>The configured <see cref="JsonSerializerOptions" />.</returns>
    public static JsonSerializerOptions AddDefaultJsonOptions(this JsonSerializerOptions options)
    {
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        options.NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals;
        options.ReadCommentHandling = JsonCommentHandling.Skip;
        options.ReferenceHandler = ReferenceHandler.IgnoreCycles;

        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        // options.Converters.Add(new GeoJsonConverterFactory());

        return options;
    }

    /// <summary>
    ///     Serializes an object to a JSON string using the specified or default options.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="obj">The object to serialize.</param>
    /// <param name="options">The <see cref="JsonSerializerOptions" /> to use, or null to use default options.</param>
    /// <returns>The JSON string representation of the object.</returns>
    public static string ToJson<T>(this T obj, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Serialize(obj, options ?? new JsonSerializerOptions().AddDefaultJsonOptions());
    }

    /// <summary>
    ///     Deserializes a JSON string to an object of the specified type using the specified or default options.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="options">The <see cref="JsonSerializerOptions" /> to use, or null to use default options.</param>
    /// <returns>The deserialized object, or the default value of <typeparamref name="T" /> if the JSON string is null.</returns>
    public static T? FromJson<T>(this string? json, JsonSerializerOptions? options = null)
    {
        return json == null
            ? default
            : JsonSerializer
                .Deserialize<T>(json, options ?? new JsonSerializerOptions().AddDefaultJsonOptions());
    }

    /// <summary>
    ///     Tries to deserialize a JSON string to an object of the specified type using the specified or default options.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="obj">
    ///     The deserialized object, or the default value of <typeparamref name="T" /> if the JSON string is null
    ///     or deserialization fails.
    /// </param>
    /// <param name="options">The <see cref="JsonSerializerOptions" /> to use, or null to use default options.</param>
    /// <returns>True if deserialization is successful; otherwise, false.</returns>
    public static bool TryFromJson<T>(this string? json, out T? obj, JsonSerializerOptions? options = null)
    {
        try
        {
            obj = json.FromJson<T>(options);
            return true;
        }
        catch
        {
            obj = default;
            return false;
        }
    }
}