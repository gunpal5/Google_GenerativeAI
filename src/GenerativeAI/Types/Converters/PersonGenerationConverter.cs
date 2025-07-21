using System.Text.Json;
using System.Text.Json.Serialization;

namespace GenerativeAI.Types.Converters;

/// <summary>
/// Converts a <see cref="VideoPersonGeneration"/> enum to and from its snake_case string representation.
/// </summary>
public class VideoPersonGenerationConverter : JsonConverter<VideoPersonGeneration>
{
    /// <summary>
    /// Reads the JSON representation (snake_case string) and converts it to a <see cref="VideoPersonGeneration"/> enum.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <param name="options">An object that specifies serialization options to use.</param>
    /// <returns>The converted <see cref="VideoPersonGeneration"/>.</returns>
    /// <exception cref="JsonException">Thrown if the JSON token is not a string or if the string value is not a valid snake_case representation.</exception>
    public override VideoPersonGeneration Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        // This converter expects a non-null string token for valid enum values.
        // Null JSON values should be handled by the serializer for nullable properties directly.
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Expected string for VideoPersonGeneration, got {reader.TokenType}");
        }

        string? value = reader.GetString();
        switch (value)
        {
            case "dont_allow":
                return VideoPersonGeneration.DontAllow;
            case "allow_adult":
                return VideoPersonGeneration.AllowAdult;
            case "allow_all":
                return VideoPersonGeneration.AllowAll;
            default:
                throw new JsonException($"Unknown or invalid VideoPersonGeneration string: {value}");
        }
    }

    /// <summary>
    /// Writes the <see cref="VideoPersonGeneration"/> enum as its snake_case string representation to JSON.
    /// </summary>
    /// <param name="writer">The writer to write to.</param>
    /// <param name="value">The value to convert to JSON.</param>
    /// <param name="options">An object that specifies serialization options to use.</param>
    /// <exception cref="JsonException">Thrown for undefined enum values.</exception>
    public override void Write(Utf8JsonWriter writer, VideoPersonGeneration value, JsonSerializerOptions options)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(writer);
#else
        if (writer == null) throw new ArgumentNullException(nameof(writer));
#endif
        string stringValue = value switch
        {
            VideoPersonGeneration.DontAllow => "dont_allow",
            VideoPersonGeneration.AllowAdult => "allow_adult",
            VideoPersonGeneration.AllowAll => "allow_all",
            _ => throw new JsonException($"Undefined VideoPersonGeneration value: {value}")
        };
        writer.WriteStringValue(stringValue);
    }
}