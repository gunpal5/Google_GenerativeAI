using System.Text.Json;
using System.Text.Json.Serialization;

namespace GenerativeAI.Types.Converters;

/// <summary>
/// Lenient JSON converter for TrafficType that handles unknown enum values gracefully.
/// When an unknown string value is encountered, it falls back to
/// <see cref="TrafficType.TRAFFIC_TYPE_UNSPECIFIED"/> instead of throwing an exception.
/// This prevents crashes when Google adds new TrafficType values to their API.
/// </summary>
public class LenientTrafficTypeConverter : JsonConverter<TrafficType>
{
    /// <summary>
    /// Reads and converts the JSON to TrafficType.
    /// </summary>
    public override TrafficType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Expected string value for TrafficType, got {reader.TokenType}");
        }

        var value = reader.GetString();
        if (string.IsNullOrEmpty(value))
        {
            return TrafficType.TRAFFIC_TYPE_UNSPECIFIED;
        }

        if (Enum.TryParse<TrafficType>(value, ignoreCase: true, out var result))
        {
            return result;
        }

        return TrafficType.TRAFFIC_TYPE_UNSPECIFIED;
    }

    /// <summary>
    /// Writes the TrafficType value as JSON.
    /// </summary>
    public override void Write(Utf8JsonWriter writer, TrafficType value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
