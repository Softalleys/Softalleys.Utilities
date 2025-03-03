using System.Text.Json;
using System.Text.Json.Serialization;

namespace Softalleys.Utilities.Json;

/// <summary>
///     Converts arrays of strings to and from space-separated values in JSON.
/// </summary>
public class SpaceSeparatedValuesConverter : JsonConverter<string[]>
{
    /// <summary>
    ///     Reads a space-separated string from JSON and converts it to an array of strings.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="typeToConvert">The type to convert to.</param>
    /// <param name="options">Serialization options.</param>
    /// <returns>An array of strings.</returns>
    public override string[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetString()?.Split(' ');
    }

    /// <summary>
    ///     Writes an array of strings to JSON as a space-separated string.
    /// </summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="value">The array of strings to write.</param>
    /// <param name="options">Serialization options.</param>
    public override void Write(Utf8JsonWriter writer, string[] value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(string.Join(' ', value));
    }
}