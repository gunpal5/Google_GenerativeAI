using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// The definition of the Rag resource.
/// </summary>
public class VertexRagStoreRagResource
{
    /// <summary>
    /// Optional. RagCorpora resource name. Format: `projects/{project}/locations/{location}/ragCorpora/{rag_corpus}`
    /// </summary>
    [JsonPropertyName("ragCorpus")]
    public string? RagCorpus { get; set; } 

    /// <summary>
    /// Optional. rag_file_id. The files should be in the same rag_corpus set in rag_corpus field.
    /// </summary>
    [JsonPropertyName("ragFileIds")]
    public System.Collections.Generic.ICollection<string>? RagFileIds { get; set; } 
}