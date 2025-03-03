using System.Text.Json;
using System.Text.Json.Serialization;
using Softalleys.Utilities.Extensions;

namespace Softalleys.Utilities.Json;

/// <summary>
///     A JSON converter for handling the serialization and deserialization of byte arrays as Base64 URL-encoded strings.
/// </summary>
public class Base64UrlTextEncoderConverter : JsonConverter<byte[]?>
{
    /// <summary>
    ///     Reads and converts the JSON token to a byte array.
    /// </summary>
    /// <param name="reader">The reader to read the JSON token from.</param>
    /// <param name="typeToConvert">The type of object to convert.</param>
    /// <param name="options">Options for the serializer.</param>
    /// <returns>A byte array if the token is a string, otherwise null.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the token is not a string or null.</exception>
    public override byte[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.String => HttpServerUtility.UrlTokenDecode(reader.GetString()!),
            JsonTokenType.Null => null,
            _ => throw new InvalidOperationException($"Invalid token type: {reader.TokenType}")
        };
    }

    /// <summary>
    ///     Writes a byte array as a Base64 URL-encoded string to the JSON writer.
    /// </summary>
    /// <param name="writer">The writer to write the JSON token to.</param>
    /// <param name="value">The byte array to write.</param>
    /// <param name="options">Options for the serializer.</param>
    public override void Write(Utf8JsonWriter writer, byte[]? value, JsonSerializerOptions options)
    {
        if (value != null)
            writer.WriteStringValue(HttpServerUtility.UrlTokenEncode(value));
        else
            writer.WriteNullValue();
    }
}