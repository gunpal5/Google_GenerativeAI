using System.Text.Json;
using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Metadata describes the input document content. Set this in `Part.document_metadata` or
/// `FunctionResponsePart.document_metadata` when `inline_data` or `file_data` contains a document to configure
/// static document preprocessing.
/// </summary>
/// <remarks>Generated from python-genai by scripts/drift/update_dtos.py.</remarks>
public class DocumentMetadata
{
    /// <summary>
    /// Content layers to return. If empty, defaults to [PAGE_IMAGE] when DocumentMetadata is set. When
    /// DocumentMetadata is not set, default document processing is used (for example, page images for PDFs).
    /// </summary>
    [JsonPropertyName("contentLayers")]
    public List<DocumentMetadataContentLayer>? ContentLayers { get; set; }

    /// <summary>
    /// 1-indexed inclusive end page.
    /// </summary>
    [JsonPropertyName("endPage")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long? EndPage { get; set; }

    /// <summary>
    /// 1-indexed inclusive start page.
    /// </summary>
    [JsonPropertyName("startPage")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long? StartPage { get; set; }
}
