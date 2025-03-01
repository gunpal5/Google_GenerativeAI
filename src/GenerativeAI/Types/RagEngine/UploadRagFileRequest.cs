using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// Request message for VertexRagDataService.UploadRagFile.
/// </summary>
public class UploadRagFileRequest
{
    /// <summary>
    /// Required. The RagFile to upload.
    /// </summary>
    [JsonPropertyName("ragFile")]
    public RagFile? RagFile { get; set; } 

    /// <summary>
    /// Required. The config for the RagFiles to be uploaded into the RagCorpus. VertexRagDataService.UploadRagFile.
    /// </summary>
    [JsonPropertyName("uploadRagFileConfig")]
    public UploadRagFileConfig? UploadRagFileConfig { get; set; } 
}