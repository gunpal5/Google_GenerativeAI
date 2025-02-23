namespace GenerativeAI.Types;

/// <summary>
/// Represents the parameters for the Image Captioning API.
/// </summary>
/// <seealso href="https://cloud.google.com/vertex-ai/generative-ai/docs/model-reference/image-captioning">See Official API Documentation</seealso>
public class ImageCaptioningParameters
{
    /// <summary>
    /// Gets or sets the number of generated text strings.  Must be between 1 and 3.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("sampleCount")]
    public int? SampleCount { get; set; }

    /// <summary>
    /// Gets or sets the Cloud Storage location to save the generated text responses.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("storageUri")]
    public string? StorageUri { get; set; }

    /// <summary>
    /// Gets or sets the language for the generated captions.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("language")]
    public string? Language { get; set; }

    /// <summary>
    ///  Gets or sets the seed for random number generator (RNG). If RNG seed is the same for requests with the inputs, the prediction results will be the same.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("seed")]
    public int? Seed { get; set; }
}