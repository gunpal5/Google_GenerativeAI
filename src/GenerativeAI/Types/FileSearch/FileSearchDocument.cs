using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// A document inside a <see cref="FileSearchStore"/>: a file that has been chunked and embedded for retrieval.
/// </summary>
/// <seealso href="https://ai.google.dev/api/file-search/documents#Document">See Official API Documentation</seealso>
public class FileSearchDocument
{
    /// <summary>
    /// Immutable. Identifier. The document resource name, for example
    /// <c>fileSearchStores/{file_search_store_id}/documents/my-awesome-doc-123a456b789c</c>.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Optional. The human-readable display name for the document (at most 512 characters).
    /// </summary>
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    /// <summary>
    /// Output only. Current processing state of the document.
    /// </summary>
    [JsonPropertyName("state")]
    public FileSearchDocumentState? State { get; set; }

    /// <summary>
    /// Output only. The size of raw bytes ingested into the document.
    /// </summary>
    [JsonPropertyName("sizeBytes")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long? SizeBytes { get; set; }

    /// <summary>
    /// Output only. The MIME type of the document.
    /// </summary>
    [JsonPropertyName("mimeType")]
    public string? MimeType { get; set; }

    /// <summary>
    /// Output only. The timestamp of when the document was created.
    /// </summary>
    [JsonPropertyName("createTime")]
    public Timestamp? CreateTime { get; set; }

    /// <summary>
    /// Output only. The timestamp of when the document was last updated.
    /// </summary>
    [JsonPropertyName("updateTime")]
    public Timestamp? UpdateTime { get; set; }

    /// <summary>
    /// Optional. User provided custom metadata stored as key-value pairs, usable in
    /// <see cref="FileSearch.MetadataFilter"/>. A document can have at most 20 entries.
    /// </summary>
    [JsonPropertyName("customMetadata")]
    public List<CustomMetadata>? CustomMetadata { get; set; }
}

/// <summary>
/// Processing state of a <see cref="FileSearchDocument"/>.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<FileSearchDocumentState>))]
public enum FileSearchDocumentState
{
    /// <summary>
    /// The default value. Used if the state is omitted.
    /// </summary>
    STATE_UNSPECIFIED = 0,

    /// <summary>
    /// Some chunks of the document are still being processed (embedding and vector storage).
    /// </summary>
    STATE_PENDING = 1,

    /// <summary>
    /// All chunks of the document are processed and available for querying.
    /// </summary>
    STATE_ACTIVE = 2,

    /// <summary>
    /// Some chunks of the document failed processing.
    /// </summary>
    STATE_FAILED = 3,
}
