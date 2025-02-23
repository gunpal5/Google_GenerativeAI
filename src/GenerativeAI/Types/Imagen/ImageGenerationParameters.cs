namespace GenerativeAI.Types;

/// <summary>
/// Represents the parameters for image generation.
/// </summary>
/// <seealso href="https://cloud.google.com/vertex-ai/generative-ai/docs/model-reference/imagen-api">See Official API Documentation</seealso>
public class ImageGenerationParameters
{
    /// <summary>
    /// Gets or sets the number of images to generate. The default value is 4.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("sampleCount")]
    public int? SampleCount { get; set; }

    /// <summary>
    /// Gets or sets the random seed for image generation.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("seed")]
    public uint? Seed { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to enhance the prompt using an LLM.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("enhancePrompt")]
    public bool? EnhancePrompt { get; set; }

    /// <summary>
    /// Gets or sets a description of what to discourage in the generated images.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("negativePrompt")]
    public string? NegativePrompt { get; set; }

    /// <summary>
    /// Gets or sets the aspect ratio for the image. The default value is "1:1".
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("aspectRatio")]
    public string? AspectRatio { get; set; }

    /// <summary>
    /// Gets or sets the output options for the image.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("outputOptions")]
    public OutputOptions? OutputOptions { get; set; }

    /// <summary>
    /// Gets or sets the style for the generated images. Only for <c>imagegeneration@002</c>.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("sampleImageStyle")]
    public string? SampleImageStyle { get; set; }

    /// <summary>
    /// Gets or sets the setting for allowing generation of people by the model.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("personGeneration")]
    public PersonGeneration? PersonGeneration { get; set; }

    /// <summary>
    /// Gets or sets the filter level for safety filtering.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("safetySetting")]
    public ImageSafetySetting? SafetySetting { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to add an invisible watermark to the generated images.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("addWatermark")]
    public bool? AddWatermark { get; set; }

    /// <summary>
    /// Gets or sets the Cloud Storage URI to store the generated images.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("storageUri")]
    public string? StorageUri { get; set; }

    /// <summary>
    /// Gets or sets the generation mode.  Must be set to <c>"upscale"</c> for upscaling requests.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("mode")]
    public string? Mode { get; set; }

    /// <summary>
    /// Gets or sets the configuration for upscaling the image.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("upscaleConfig")]
    public UpscaleConfig? UpscaleConfig { get; set; }
}