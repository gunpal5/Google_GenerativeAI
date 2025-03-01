using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// A RagFile contains user data for chunking, embedding and indexing.
/// </summary>
public class RagFile
{
    /// <summary>
    /// Output only. Timestamp when this RagFile was created.
    /// </summary>
    [JsonPropertyName("createTime")]
    public string? CreateTime { get; set; }

    /// <summary>
    /// Optional. The description of the RagFile.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Output only. The RagFile is encapsulated and uploaded in the UploadRagFile request.
    /// </summary>
    [JsonPropertyName("directUploadSource")]
    public DirectUploadSource? DirectUploadSource { get; set; }

    /// <summary>
    /// Required. The display name of the RagFile. The name can be up to 128 characters long and can consist of any UTF-8 characters.
    /// </summary>
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    /// <summary>
    /// Output only. State of the RagFile.
    /// </summary>
    [JsonPropertyName("fileStatus")]
    public FileStatus? FileStatus { get; set; }

    /// <summary>
    /// Output only. Google Cloud Storage location of the RagFile. It does not support wildcards in the Cloud Storage uri for now.
    /// </summary>
    [JsonPropertyName("gcsSource")]
    public GcsSource? GcsSource { get; set; }

    /// <summary>
    /// Output only. Google Drive location. Supports importing individual files as well as Google Drive folders.
    /// </summary>
    [JsonPropertyName("googleDriveSource")]
    public GoogleDriveSource? GoogleDriveSource { get; set; }

    /// <summary>
    /// The RagFile is imported from a Jira query.
    /// </summary>
    [JsonPropertyName("jiraSource")]
    public JiraSource? JiraSource { get; set; }

    /// <summary>
    /// Output only. The resource name of the RagFile.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Output only. The type of the RagFile.
    /// </summary>
    [JsonPropertyName("ragFileType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RagFileType? RagFileType { get; set; }

    /// <summary>
    /// The RagFile is imported from a SharePoint source.
    /// </summary>
    [JsonPropertyName("sharePointSources")]
    public SharePointSources? SharePointSources { get; set; }

    /// <summary>
    /// Output only. The size of the RagFile in bytes.
    /// </summary>
    [JsonPropertyName("sizeBytes")]
    public string? SizeBytes { get; set; }

    /// <summary>
    /// The RagFile is imported from a Slack channel.
    /// </summary>
    [JsonPropertyName("slackSource")]
    public SlackSource? SlackSource { get; set; }

    /// <summary>
    /// Output only. Timestamp when this RagFile was last updated.
    /// </summary>
    [JsonPropertyName("updateTime")]
    public string? UpdateTime { get; set; }
}