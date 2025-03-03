using System.Text.Json;
using System.Text.Json.Serialization;

namespace Softalleys.Utilities.Json;

/// <summary>
///     A JSON converter that can handle both single string values and arrays of strings during serialization and
///     deserialization.
/// </summary>
public class SingleOrArrayConverter<T> : JsonConverter<T[]>
{
    /// <summary>
    ///     Deserializes JSON data into an array of strings.
    ///     If the JSON data is a single string, it returns an array with one element.
    ///     If the JSON data is an array of strings, it converts each element and returns them in an array.
    /// </summary>
    /// <param name="reader">The reader to read the JSON data from.</param>
    /// <param name="typeToConvert">The type to convert to, expected to be a string array.</param>
    /// <param name="options">Options for the JSON serializer.</param>
    /// <returns>An array of strings parsed from the JSON data.</returns>
    /// <exception cref="JsonException">Thrown if an unexpected token type is encountered.</exception>
    public override T[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var elementType = typeof(T);
        var converter = (JsonConverter<T>)options.GetConverter(elementType)
                        ?? throw new JsonException($"No converter found for {elementType}");

        if (reader.TokenType == JsonTokenType.Null)
        {
            reader.Read();
            return null;
        }

        if (reader.TokenType == JsonTokenType.String)
            return new[] { ReadFrom(ref reader, elementType, converter, options) };

        if (reader.TokenType != JsonTokenType.StartArray) throw new JsonException("Unexpected token type.");

        var values = new List<T>();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray) return values.ToArray();

            values.Add(ReadFrom(ref reader, elementType, converter, options));
        }

        throw new JsonException("Unexpected end of JSON array.");
    }

    private static T ReadFrom(ref Utf8JsonReader reader, Type elementType, JsonConverter<T> converter,
        JsonSerializerOptions options)
    {
        return converter.Read(ref reader, elementType, options)
               ?? throw new JsonException("Null values are not allowed");
    }

    /// <summary>
    ///     Serializes an array of strings to JSON.
    ///     If the array contains a single string, it writes it as a single string value.
    ///     If the array contains multiple strings, it writes them as an array of strings.
    /// </summary>
    /// <param name="writer">The writer to write the JSON data to.</param>
    /// <param name="value">The array of strings to write.</param>
    /// <param name="options">Options for the JSON serializer.</param>
    /// <exception cref="ArgumentNullException">Thrown if the writer or value is null.</exception>
    public override void Write(Utf8JsonWriter writer, T[]? value, JsonSerializerOptions options)
    {
        var elementType = typeof(T);
        var converter = (JsonConverter<T>)options.GetConverter(elementType)
                        ?? throw new JsonException($"No converter found for {elementType}");

        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        if (value.Length == 1)
        {
            converter.Write(writer, value[0], options);
        }
        else
        {
            writer.WriteStartArray();
            foreach (var item in value) converter.Write(writer, item, options);
            writer.WriteEndArray();
        }
    }
}