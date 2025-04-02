using System.Text.Json.Serialization;
using GenerativeAI.Types.Converters;

namespace GenerativeAI.Types;

    /// <summary>
    /// Defines the possible aspect ratios for generated videos.
    /// </summary>
    /// <seealso href="https://ai.google.dev/docs/video">See Official API Documentation</seealso>
    [JsonConverter(typeof(VideoAspectRatioConverter))]
    public enum VideoAspectRatio
    {
        /// <summary>
        /// Unspecified aspect ratio. The model may choose a default.
        /// </summary>
        ASPECT_RATIO_UNSPECIFIED,
        /// <summary>
        /// Landscape aspect ratio (16:9).
        /// </summary>
        LANDSCAPE_16_9,
        /// <summary>
        /// Portrait aspect ratio (9:16).
        /// </summary>
        PORTRAIT_9_16,
        /// <summary>
        /// Square aspect ratio (1:1).
        /// </summary>
        SQUARE_1_1
    }