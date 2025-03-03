using System.Text.Json;
using System.Text.Json.Serialization;

namespace Softalleys.Utilities.Json;

/// <summary>
///     Converts TimeSpan objects to and from JSON as a number of seconds.
/// </summary>
public class TimeSpanSecondsConverter : JsonConverter<TimeSpan>
{
    /// <summary>
    ///     Reads a JSON number representing seconds and converts it to a TimeSpan.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="typeToConvert">The type to convert to.</param>
    /// <param name="options">Serialization options.</param>
    /// <returns>A TimeSpan object.</returns>
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return TimeSpan.FromSeconds(
            reader.TokenType switch
            {
                JsonTokenType.String when long.TryParse(reader.GetString(), out var parsed) => parsed,
                JsonTokenType.Number => reader.GetInt64(),
                _ => throw new JsonException()
            });
    }

    /// <summary>
    ///     Writes a TimeSpan to JSON as a number of seconds.
    /// </summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="value">The TimeSpan value.</param>
    /// <param name="options">Serialization options.</param>
    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue((long)value.TotalSeconds);
    }
}