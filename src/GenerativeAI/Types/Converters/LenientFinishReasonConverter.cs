using System.Text.Json;
using System.Text.Json.Serialization;

namespace GenerativeAI.Types.Converters;

/// <summary>
/// Lenient JSON converter for FinishReason that handles unknown enum values gracefully.
/// When an unknown string value is encountered, it falls back to FinishReason.OTHER
/// instead of throwing an exception. This prevents crashes when Google adds new
/// FinishReason values to their API.
/// </summary>
public class LenientFinishReasonConverter : JsonConverter<FinishReason>
{
    /// <summary>
    /// Reads and converts the JSON to FinishReason.
    /// </summary>
    public override FinishReason Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Expected string value for FinishReason, got {reader.TokenType}");
        }

        var value = reader.GetString();
        if (string.IsNullOrEmpty(value))
        {
            return FinishReason.FINISH_REASON_UNSPECIFIED;
        }

        if (Enum.TryParse<FinishReason>(value, ignoreCase: true, out var result))
        {
            return result;
        }

        return FinishReason.OTHER;
    }

    /// <summary>
    /// Writes the FinishReason value as JSON.
    /// </summary>
    public override void Write(Utf8JsonWriter writer, FinishReason value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
