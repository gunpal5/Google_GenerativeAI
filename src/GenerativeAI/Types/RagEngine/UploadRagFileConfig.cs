using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// Config for uploading RagFile.
/// </summary>
public class UploadRagFileConfig
{
   
    /// <summary>
    /// Specifies the transformation config for RagFiles.
    /// </summary>
    [JsonPropertyName("ragFileTransformationConfig")]
    public RagFileTransformationConfig? RagFileTransformationConfig { get; set; } 
}