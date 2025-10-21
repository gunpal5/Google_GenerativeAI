using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// The Google Cloud Storage location where the output is to be written to.
/// </summary>
public class GcsDestination
{
    /// <summary>
    /// Required. Google Cloud Storage URI to output directory.
    /// If the uri doesn't end with '/', a '/' will be automatically appended.
    /// The directory is created if it doesn't exist.
    /// Must be a valid GCS path starting with "gs://".
    /// </summary>
    [JsonPropertyName("outputUriPrefix")]
    public string? OutputUriPrefix { get; set; }
}
