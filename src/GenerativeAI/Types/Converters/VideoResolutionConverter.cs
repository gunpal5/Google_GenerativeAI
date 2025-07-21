using System.Text.Json;
using System.Text.Json.Serialization;

namespace GenerativeAI.Types.Converters;

 /// <summary>
    /// Converts a <see cref="VideoResolution"/> enum to and from its string representation (e.g., "HD_720P", "SD_480P").
    /// Converts <see cref="VideoResolution.RESOLUTION_UNSPECIFIED"/> to/from JSON null.
    /// </summary>
    /// <seealso href="https://ai.google.dev/docs/video">See Official API Documentation</seealso> // Link might need refinement
    public class VideoResolutionConverter : JsonConverter<VideoResolution>
    {
        /// <summary>
        /// Reads the JSON representation and converts it to a <see cref="VideoResolution"/> enum.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        /// <returns>The converted <see cref="VideoResolution"/>.</returns>
        /// <exception cref="JsonException">Thrown if an unknown resolution string is encountered.</exception>
        public override VideoResolution Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return VideoResolution.RESOLUTION_UNSPECIFIED;
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                string? value = reader.GetString();
                switch (value)
                {
                    case "720x1280":
                    case "1280x720":
                    case "720p":
                        return VideoResolution.HD_720P;
                    case "1920x1080":
                    case "1080x1920":
                    case "1080p":
                        return VideoResolution.FullHD_1080P;
                    case "480x640":
                    case "640x480":
                    case "480p":
                        return VideoResolution.SD_480P;
                    default:
                        return VideoResolution.RESOLUTION_UNSPECIFIED;
                }
            }

            throw new JsonException($"Expected string or null for VideoResolution, got {reader.TokenType}");
        }

        /// <summary>
        /// Writes the <see cref="VideoResolution"/> enum as its specific string representation to JSON.
        /// Writes null for <see cref="VideoResolution.RESOLUTION_UNSPECIFIED"/>.
        /// </summary>
        /// <param name="writer">The writer to write to.</param>
        /// <param name="value">The value to convert to JSON.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        public override void Write(Utf8JsonWriter writer, VideoResolution value, JsonSerializerOptions options)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(writer);
#else
            if (writer == null) throw new ArgumentNullException(nameof(writer));
#endif
            switch (value)
            {
                case VideoResolution.HD_720P:
                    writer.WriteStringValue("720p");
                    break;
                case VideoResolution.FullHD_1080P:
                    writer.WriteStringValue("1080p");
                    break;
                case VideoResolution.SD_480P:
                    writer.WriteStringValue("480p");
                    break;
                default:
                    writer.WriteNullValue();
                    break;
            }
        }
    }