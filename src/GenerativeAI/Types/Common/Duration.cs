using System.Text.Json;
using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// A Duration represents a signed, fixed-length span of time represented as a count of seconds and fractions
/// of seconds at nanosecond resolution.
/// </summary>
/// <seealso href="https://developers.google.com/protocol-buffers/docs/reference/google.protobuf#google.protobuf.Duration">See Official Protobuf Documentation</seealso>
[JsonConverter(typeof(DurationJsonConverter))]
public class Duration
{
    /// <summary>
    /// Signed seconds of the span of time. Must be from -315,576,000,000 to +315,576,000,000 inclusive.
    /// </summary>
    public long Seconds { get; set; }

    /// <summary>
    /// Signed fractions of a second at nanosecond resolution of the span of time. Durations less than one second are
    /// represented with a 0 <see cref="Seconds"/> field and a positive or negative <see cref="Nanos"/> field. For durations
    /// of one second or more, a non-zero value for the <see cref="Nanos"/> field must be of the same sign as the <see cref="Seconds"/>
    /// field. Must be from -999,999,999 to +999,999,999 inclusive.
    /// </summary>
    public int Nanos { get; set; }

    /// <summary>
    /// Implicitly converts a <see cref="TimeSpan"/> object to a <see cref="Duration"/> object.
    /// </summary>
    /// <param name="timeSpan">The <see cref="TimeSpan"/> object to convert.</param>
    public static implicit operator Duration(TimeSpan timeSpan)
    {
        return Duration.FromTimeSpan(timeSpan);
    }

    /// <summary>
    /// Implicitly converts a <see cref="Duration"/> object to a <see cref="TimeSpan"/> object.
    /// </summary>
    /// <param name="duration">The <see cref="Duration"/> object to convert.</param>
    public static implicit operator TimeSpan(Duration duration)
    {
        return duration.ToTimeSpan();
    }

    /// <summary>
    /// Converts this <see cref="Duration"/> object to a <see cref="TimeSpan"/> object.
    /// </summary>
    /// <returns>The converted <see cref="TimeSpan"/> object.</returns>
    public TimeSpan ToTimeSpan()
    {
        return TimeSpan.FromSeconds(Seconds) + TimeSpan.FromTicks(Nanos / 100);
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> object from a <see cref="TimeSpan"/> object.
    /// </summary>
    /// <param name="timeSpan">The <see cref="TimeSpan"/> object to convert.</param>
    /// <returns>The new <see cref="Duration"/> object.</returns>
    public static Duration FromTimeSpan(TimeSpan timeSpan)
    {
        return new Duration
        {
            Seconds = (long)timeSpan.TotalSeconds,
            Nanos = (int)(timeSpan.Ticks % TimeSpan.TicksPerSecond) * 100,
        };
    }
}
/// <summary>
/// JSON converter for <see cref="Duration"/>.
/// </summary>
public class DurationJsonConverter : JsonConverter<Duration>
{
    /// <inheritdoc/>
    public override Duration Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Parse the duration string (e.g., "3.5s")
        var durationString = reader.GetString();
        var duration = double.Parse(durationString!.TrimEnd('s'));

        // Convert the duration to seconds and nanoseconds
        var seconds = (long)duration;
        var nanos = (int)((duration - seconds) * 1_000_000_000);

        return new Duration { Seconds = seconds, Nanos = nanos };
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, Duration value, JsonSerializerOptions options)
    {
        // Convert seconds and nanoseconds to a duration string with the specified format
        var duration = (double)value.Seconds + (double)value.Nanos / 1_000_000_000;
        writer.WriteStringValue($"{duration:F9}s");
    }
}