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
    private const string BatchStatePrefix = "BATCH_STATE_";
    private const string JobStatePrefix = "JOB_STATE_";

    /// <summary>
    /// Reads a <see cref="JobState"/> value, accepting both BATCH_STATE_* and JOB_STATE_* names.
    /// </summary>
    /// <param name="reader">The reader positioned at the value.</param>
    /// <param name="typeToConvert">The type being converted.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>The parsed state, or <see cref="JobState.JOB_STATE_UNSPECIFIED"/> for unknown values.</returns>
    public override JobState Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Expected string value for JobState, got {reader.TokenType}");
        }

        var value = reader.GetString();
        if (value is null || value.Length == 0)
        {
            return JobState.JOB_STATE_UNSPECIFIED;
        }

        // Handle BATCH_STATE_* format from Google AI
        if (value.StartsWith(BatchStatePrefix, StringComparison.Ordinal))
        {
#if NET6_0_OR_GREATER
            value = string.Concat(JobStatePrefix, value.AsSpan(BatchStatePrefix.Length));
#else
            value = JobStatePrefix + value.Substring(BatchStatePrefix.Length);
#endif
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

    /// <summary>
    /// Writes a <see cref="JobState"/> value as its JOB_STATE_* name.
    /// </summary>
    /// <param name="writer">The writer to write to.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="options">The serializer options.</param>
    public override void Write(Utf8JsonWriter writer, JobState value, JsonSerializerOptions options)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(writer);
#else
        if (writer == null) throw new ArgumentNullException(nameof(writer));
#endif
        writer.WriteStringValue(value.ToString());
    }
}
