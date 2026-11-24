using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Content layers to extract from a document.
/// </summary>
/// <remarks>Generated from python-genai by scripts/drift/update_dtos.py.</remarks>
[JsonConverter(typeof(JsonStringEnumConverter<DocumentMetadataContentLayer>))]
public enum DocumentMetadataContentLayer
{
    /// <summary>
    /// The API value "CONTENT_LAYER_UNSPECIFIED".
    /// </summary>
    CONTENT_LAYER_UNSPECIFIED = 0,

    /// <summary>
    /// The API value "PAGE_IMAGE".
    /// </summary>
    PAGE_IMAGE = 1,

    /// <summary>
    /// The API value "PAGE_TEXT".
    /// </summary>
    PAGE_TEXT = 2
}
