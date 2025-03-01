using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// Response message for VertexRagDataService.UploadRagFile.
/// </summary>
public class UploadRagFileResponse
{
    /// <summary>
    /// The error that occurred while processing the RagFile.
    /// </summary>
    [JsonPropertyName("error")]
    public GoogleRpcStatus? Error { get; set; } 

    /// <summary>
    /// The RagFile that had been uploaded into the RagCorpus.
    /// </summary>
    [JsonPropertyName("ragFile")]
    public RagFile? RagFile { get; set; } 

   
}