using System.Text.Json;
using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// A Timestamp represents a point in time independent of any time zone or calendar,
/// represented as seconds and fractions of seconds at nanosecond resolution in UTC Epoch time.
/// </summary>
/// <seealso href="https://developers.google.com/protocol-buffers/docs/reference/google.protobuf#google.protobuf.Timestamp">See Official Protobuf Documentation</seealso>
[JsonConverter(typeof(TimestampJsonConverter))]
public class Timestamp
{
    /// <summary>
    /// Represents seconds of UTC time since Unix epoch 1970-01-01T00:00:00Z. Must be from 0001-01-01T00:00:00Z to
    /// 9999-12-31T23:59:59Z inclusive.
    /// </summary>
    public long Seconds { get; set; }

    /// <summary>
    /// Non-negative fractions of a second at nanosecond resolution. Negative second values with fractions
    /// must still have non-negative nanos values that count forward in time. Must be from 0 to 999,999,999
    /// inclusive.
    /// </summary>
    public int Nanos { get; set; }

    /// <summary>
    /// Implicitly converts a <see cref="DateTime"/> object to a <see cref="Timestamp"/> object.
    /// </summary>
    /// <param name="dateTime">The <see cref="DateTime"/> object to convert.</param>
    public static implicit operator Timestamp(DateTime dateTime)
    {
        return Timestamp.FromDateTime(dateTime);
    }

    /// <summary>
    /// Implicitly converts a <see cref="Timestamp"/> object to a <see cref="DateTime"/> object.
    /// </summary>
    /// <param name="timestamp">The <see cref="Timestamp"/> object to convert.</param>
    public static implicit operator DateTime(Timestamp timestamp)
    {
        return timestamp.ToDateTime();
    }

    /// <summary>
    /// Converts this <see cref="Timestamp"/> object to a <see cref="DateTime"/> object.
    /// </summary>
    /// <returns>The converted <see cref="DateTime"/> object.</returns>
    public DateTime ToDateTime()
    {
        return DateTimeOffset.FromUnixTimeSeconds(Seconds).AddTicks(Nanos / 100).UtcDateTime;
    }

    /// <summary>
    /// Creates a new <see cref="Timestamp"/> object from a <see cref="DateTime"/> object.
    /// </summary>
    /// <param name="dateTime">The <see cref="DateTime"/> object to convert.</param>
    /// <returns>The new <see cref="Timestamp"/> object.</returns>
    public static Timestamp FromDateTime(DateTime dateTime)
    {
        var dateTimeOffset = new DateTimeOffset(dateTime);
        return new Timestamp
        {
            Seconds = dateTimeOffset.ToUnixTimeSeconds(),
            Nanos = (int)(dateTimeOffset.Ticks % TimeSpan.TicksPerSecond) * 100,
        };
    }
}

/// <summary>
/// JSON converter for <see cref="Timestamp"/>.
/// </summary>
public class TimestampJsonConverter: JsonConverter<Timestamp>
{
    /// <inheritdoc/>
    public override Timestamp Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Assuming the JSON representation is a string in RFC 3339 format
        var timestampString = reader.GetString();
        return Timestamp.FromDateTime(DateTime.Parse(timestampString));
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, Timestamp value, JsonSerializerOptions options)
    {
        // Assuming the JSON representation is a string in RFC 3339 format
        writer.WriteStringValue(value.ToDateTime().ToString("o")); // "o" format specifier for RFC 3339
    }
}
