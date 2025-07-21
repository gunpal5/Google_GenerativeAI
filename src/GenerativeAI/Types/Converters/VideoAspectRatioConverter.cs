namespace GenerativeAI.Types.Converters;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Converts a <see cref="VideoAspectRatio"/> enum to and from its string representation (e.g., "16:9", "1:1").
/// Converts <see cref="VideoAspectRatio.ASPECT_RATIO_UNSPECIFIED"/> to/from JSON null.
/// </summary>
/// <seealso href="https://ai.google.dev/docs/video">See Official API Documentation</seealso>
public class VideoAspectRatioConverter : JsonConverter<VideoAspectRatio>
{
    /// <summary>
    /// Reads the JSON representation and converts it to a <see cref="VideoAspectRatio"/> enum.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <param name="options">An object that specifies serialization options to use.</param>
    /// <returns>The converted <see cref="VideoAspectRatio"/>.</returns>
    /// <exception cref="JsonException">Thrown if an unknown aspect ratio string is encountered.</exception>
    public override VideoAspectRatio Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return VideoAspectRatio.ASPECT_RATIO_UNSPECIFIED;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            string? value = reader.GetString();
            switch (value)
            {
                case "16:9":
                    return VideoAspectRatio.LANDSCAPE_16_9;
                case "9:16":
                    return VideoAspectRatio.PORTRAIT_9_16;
                case "1:1":
                    return VideoAspectRatio.SQUARE_1_1;
                default:
                    return VideoAspectRatio.ASPECT_RATIO_UNSPECIFIED;
            }
        }

        throw new JsonException($"Expected string or null for VideoAspectRatio, got {reader.TokenType}");
    }

    /// <summary>
    /// Writes the <see cref="VideoAspectRatio"/> enum as its specific string representation to JSON.
    /// Writes null for <see cref="VideoAspectRatio.ASPECT_RATIO_UNSPECIFIED"/>.
    /// </summary>
    /// <param name="writer">The writer to write to.</param>
    /// <param name="value">The value to convert to JSON.</param>
    /// <param name="options">An object that specifies serialization options to use.</param>
    public override void Write(Utf8JsonWriter writer, VideoAspectRatio value, JsonSerializerOptions options)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(writer);
#else
        if (writer == null) throw new ArgumentNullException(nameof(writer));
#endif
        switch (value)
        {
            case VideoAspectRatio.LANDSCAPE_16_9:
                writer.WriteStringValue("16:9");
                break;
            case VideoAspectRatio.PORTRAIT_9_16:
                writer.WriteStringValue("9:16");
                break;
            case VideoAspectRatio.SQUARE_1_1:
                writer.WriteStringValue("1:1");
                break;
            case VideoAspectRatio.ASPECT_RATIO_UNSPECIFIED:
                writer.WriteNullValue();
                break;
            default:
                throw new JsonException($"Unknown VideoAspectRatio value: {value}");
        }
    }
}