namespace GenerativeAI.Types;

/// <summary>
/// Represents the output options for the generated image.
/// </summary>
/// <seealso href="https://cloud.google.com/vertex-ai/generative-ai/docs/model-reference/imagen-api">See Official API Documentation</seealso>
public class OutputOptions
{
    /// <summary>
    /// Gets or sets the image format that the output should be saved as. The default value is "image/png".
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("mimeType")]
    public string? MimeType { get; set; }

    /// <summary>
    /// Gets or sets the level of compression if the output type is "image/jpeg". Accepted values are 0 through 100. The default value is 75.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("compressionQuality")]
    public int? CompressionQuality { get; set; }
}