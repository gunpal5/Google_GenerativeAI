using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// Request message for VertexRagDataService.ImportRagFiles.
/// </summary>
public class ImportRagFilesRequest
{
    /// <summary>
    /// Required. The config for the RagFiles to be synced and imported into the RagCorpus. VertexRagDataService.ImportRagFiles.
    /// </summary>
    [JsonPropertyName("importRagFilesConfig")]
    public ImportRagFilesConfig? ImportRagFilesConfig { get; set; } 
}