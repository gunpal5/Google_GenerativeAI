using System.Text.Json.Serialization;
using GenerativeAI.Types.RagEngine;

namespace GenerativeAI.Types;

public class VertexRetrievalTool
{
    /// <summary>
    /// Set to use data source powered by Vertex AI Search.
    /// </summary>
    [JsonPropertyName("vertexAiSearch")]
    public VertexAISearch? VertexAiSearch { get; set; } 

    /// <summary>
    /// Set to use data source powered by Vertex RAG store. User data is uploaded via the VertexRagDataService.
    /// </summary>
    [JsonPropertyName("vertexRagStore")]
    public VertexRagStore? VertexRagStore { get; set; } 
}