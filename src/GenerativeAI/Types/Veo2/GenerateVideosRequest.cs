using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Represents the parameters sent when requesting video generation.
/// </summary>
/// <remarks>
/// This class encapsulates the model identifier, prompt, optional input image, and configuration settings for initiating a video generation task.
/// </remarks>
public class GenerateVideosRequest
{
    /// <summary>
    /// Required. The identifier of the model to use for video generation.
    /// For a list of available models, refer to the Google AI documentation.
    /// Example: "models/gemini-1.5-flash" or a specific Veo model ID if available.
    /// </summary>
    [JsonPropertyName("model")]
    public string? Model { get; set; } // Marked as optional in Python but likely required by API

    /// <summary>
    /// Optional. The text prompt describing the desired video content.
    /// This is typically required unless an input <see cref="Image"/> is provided for image-to-video tasks.
    /// </summary>
    [JsonPropertyName("prompt")]
    public string? Prompt { get; set; }

    /// <summary>
    /// Optional. An input image to use as a basis for video generation (image-to-video).
    /// Required if <see cref="Prompt"/> is not provided for image-to-video tasks.
    /// </summary>
    [JsonPropertyName("image")]
    public ImageSample? Image { get; set; }

    /// <summary>
    /// Optional. Configuration settings for the video generation process.
    /// Includes parameters like FPS, duration, resolution, aspect ratio, etc.
    /// </summary>
    [JsonPropertyName("config")]
    public GenerateVideosConfig? Config { get; set; }
}