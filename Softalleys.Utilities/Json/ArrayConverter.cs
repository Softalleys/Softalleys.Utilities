using System.Text.Json;
using System.Text.Json.Serialization;

namespace Softalleys.Utilities.Json;

/// <summary>
///     A custom JSON converter that handles the serialization and deserialization of arrays of a specific type.
///     It uses a specified element converter for individual elements of the array.
/// </summary>
/// <typeparam name="TElement">The type of the elements in the array.</typeparam>
/// <typeparam name="TConverter">The type of the converter used for the elements in the array.</typeparam>
public class ArrayConverter<TElement, TConverter> : JsonConverter<TElement?[]?>
    where TConverter : JsonConverter<TElement>, new()
{
    private readonly TConverter _elementConverter = new();

    /// <summary>
    ///     Reads and converts the JSON to an array of type <typeparamref name="TElement" />.
    /// </summary>
    /// <param name="reader">The reader to read JSON from.</param>
    /// <param name="typeToConvert">The type of object to convert to.</param>
    /// <param name="options">Options for the serializer.</param>
    /// <returns>An array of <typeparamref name="TElement" /> or null if the JSON token is null.</returns>
    public override TElement?[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            reader.Read();
            return null;
        }

        if (reader.TokenType != JsonTokenType.StartArray) throw new JsonException();

        var result = new List<TElement?>();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray) return result.ToArray();

            var element = reader.TokenType == JsonTokenType.Null
                ? default
                : _elementConverter.Read(ref reader, typeof(TElement), options);
            result.Add(element);
        }

        throw new JsonException();
    }

    /// <summary>
    ///     Writes an array of <typeparamref name="TElement" /> to JSON.
    /// </summary>
    /// <param name="writer">The writer to write JSON to.</param>
    /// <param name="value">The array of <typeparamref name="TElement" /> to write.</param>
    /// <param name="options">Options for the serializer.</param>
    public override void Write(Utf8JsonWriter writer, TElement?[]? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartArray();
        foreach (var element in value)
            if (element is null)
                writer.WriteNullValue();
            else
                _elementConverter.Write(writer, element, options);
        writer.WriteEndArray();
    }
}