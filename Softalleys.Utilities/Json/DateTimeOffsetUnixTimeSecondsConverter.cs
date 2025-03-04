using System.Text.Json;
using System.Text.Json.Serialization;

namespace Softalleys.Utilities.Json;

/// <summary>
///     Converts <see cref="DateTimeOffset" /> objects to and from Unix time (number of seconds since Unix epoch) in JSON.
/// </summary>
public class DateTimeOffsetUnixTimeSecondsConverter : JsonConverter<DateTimeOffset>
{
    /// <summary>
    ///     Reads and converts the JSON to a <see cref="DateTimeOffset" /> object.
    /// </summary>
    /// <param name="reader">The reader to read JSON from.</param>
    /// <param name="typeToConvert">The type of object to convert to.</param>
    /// <param name="options">Options for the serializer.</param>
    /// <returns>A <see cref="DateTimeOffset" /> object.</returns>
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64());
    }

    /// <summary>
    ///     Writes a <see cref="DateTimeOffset" /> object as a Unix time (number of seconds since Unix epoch) to JSON.
    /// </summary>
    /// <param name="writer">The writer to write JSON to.</param>
    /// <param name="value">The <see cref="DateTimeOffset" /> value to write.</param>
    /// <param name="options">Options for the serializer.</param>
    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.ToUnixTimeSeconds());
    }
}