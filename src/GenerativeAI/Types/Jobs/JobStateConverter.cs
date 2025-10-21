using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// JSON converter for JobState that handles both BATCH_STATE_* and JOB_STATE_* formats.
/// Google AI uses BATCH_STATE_* which is converted to JOB_STATE_* for consistency.
/// </summary>
public class JobStateConverter : JsonConverter<JobState>
{
    public override JobState Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Expected string value for JobState, got {reader.TokenType}");
        }

        var value = reader.GetString();
        if (string.IsNullOrEmpty(value))
        {
            return JobState.JOB_STATE_UNSPECIFIED;
        }

        // Handle BATCH_STATE_* format from Google AI
        if (value.StartsWith("BATCH_STATE_"))
        {
            value = value.Replace("BATCH_STATE_", "JOB_STATE_");
        }

        // Parse the JOB_STATE_* value
        if (Enum.TryParse<JobState>(value, ignoreCase: true, out var result))
        {
            return result;
        }

        // If parsing fails, return the original string as UNSPECIFIED
        // This allows for unknown future states
        return JobState.JOB_STATE_UNSPECIFIED;
    }

    public override void Write(Utf8JsonWriter writer, JobState value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
