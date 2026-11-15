using System.Text.Json;
using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// The specification for an output Vertex AI multimodal dataset. This type is experimental and may change in
/// future versions.
/// </summary>
/// <remarks>Generated from python-genai by scripts/drift/update_dtos.py.</remarks>
public class VertexMultimodalDatasetDestination
{
    /// <summary>
    /// The BigQuery destination for the multimodal dataset.
    /// </summary>
    [JsonPropertyName("bigqueryDestination")]
    public string? BigqueryDestination { get; set; }

    /// <summary>
    /// The display name of the multimodal dataset.
    /// </summary>
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }
}
