using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// A RagCorpus is a RagFile container and a project can have multiple RagCorpora.
/// </summary>
public class RagCorpus
{
    /// <summary>
    /// Output only. RagCorpus state.
    /// </summary>
    [JsonPropertyName("corpusStatus")]
    public CorpusStatus? CorpusStatus { get; set; } 

    /// <summary>
    /// Output only. Timestamp when this RagCorpus was created.
    /// </summary>
    [JsonPropertyName("createTime")]
    public string? CreateTime { get; set; } 

    /// <summary>
    /// Optional. The description of the RagCorpus.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; } 

    /// <summary>
    /// Required. The display name of the RagCorpus. The name can be up to 128 characters long and can consist of any UTF-8 characters.
    /// </summary>
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; } 

    /// <summary>
    /// Output only. The resource name of the RagCorpus.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; } 
    

    /// <summary>
    /// Output only. Number of RagFiles in the RagCorpus.
    /// </summary>
    [JsonPropertyName("ragFilesCount")]
    public int? RagFilesCount { get; set; } 

    /// <summary>
    /// Output only. Timestamp when this RagCorpus was last updated.
    /// </summary>
    [JsonPropertyName("updateTime")]
    public string? UpdateTime { get; set; } 

    /// <summary>
    /// Optional. Immutable. The config for the Vector DBs.
    /// </summary>
    [JsonPropertyName("vectorDbConfig")]
    public RagVectorDbConfig? VectorDbConfig { get; set; } 

    /// <summary>
    /// Optional. Immutable. The config for the Vertex AI Search.
    /// </summary>
    [JsonPropertyName("vertexAiSearchConfig")]
    public VertexAiSearchConfig? VertexAiSearchConfig { get; set; } 

   
}