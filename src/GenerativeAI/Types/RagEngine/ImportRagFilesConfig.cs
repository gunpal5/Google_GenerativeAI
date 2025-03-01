using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// Config for importing RagFiles.
/// </summary>
public class ImportRagFilesConfig
{
    /// <summary>
    /// Google Cloud Storage location. Supports importing individual files as well as entire Google Cloud Storage directories. Sample formats: - `gs://bucket_name/my_directory/object_name/my_file.txt` - `gs://bucket_name/my_directory`
    /// </summary>
    [JsonPropertyName("gcsSource")]
    public GcsSource? GcsSource { get; set; } 

    /// <summary>
    /// Google Drive location. Supports importing individual files as well as Google Drive folders.
    /// </summary>
    [JsonPropertyName("googleDriveSource")]
    public GoogleDriveSource? GoogleDriveSource { get; set; } 

    /// <summary>
    /// Jira queries with their corresponding authentication.
    /// </summary>
    [JsonPropertyName("jiraSource")]
    public JiraSource? JiraSource { get; set; } 

    /// <summary>
    /// Optional. The max number of queries per minute that this job is allowed to make to the embedding model specified on the corpus. This value is specific to this job and not shared across other import jobs. Consult the Quotas page on the project to set an appropriate value here. If unspecified, a default value of 1,000 QPM would be used.
    /// </summary>
    [JsonPropertyName("maxEmbeddingRequestsPerMin")]
    public int? MaxEmbeddingRequestsPerMin { get; set; }

   
    /// <summary>
    /// Optional. Specifies the parsing config for RagFiles. RAG will use the default parser if this field is not set.
    /// </summary>
    [JsonPropertyName("ragFileParsingConfig")]
    public RagFileParsingConfig? RagFileParsingConfig { get; set; } 

    /// <summary>
    /// Specifies the transformation config for RagFiles.
    /// </summary>
    [JsonPropertyName("ragFileTransformationConfig")]
    public RagFileTransformationConfig? RagFileTransformationConfig { get; set; } 

    /// <summary>
    /// SharePoint sources.
    /// </summary>
    [JsonPropertyName("sharePointSources")]
    public SharePointSources? SharePointSources { get; set; } 

    /// <summary>
    /// Slack channels with their corresponding access tokens.
    /// </summary>
    [JsonPropertyName("slackSource")]
    public SlackSource? SlackSource { get; set; } 
}