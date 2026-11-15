using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// A File Search store: a managed collection of <see cref="FileSearchDocument"/>s that the
/// <see cref="FileSearch"/> tool can retrieve from (managed retrieval-augmented generation).
/// </summary>
/// <remarks>File Search stores are available on the Gemini Developer API only (not Vertex AI).</remarks>
/// <seealso href="https://ai.google.dev/api/file-search/file-search-stores#FileSearchStore">See Official API Documentation</seealso>
public class FileSearchStore
{
    /// <summary>
    /// Output only. Immutable. Identifier. The <see cref="FileSearchStore"/> resource name, for example
    /// <c>fileSearchStores/my-awesome-file-search-store-123a456b789c</c>. The ID is derived from
    /// <see cref="DisplayName"/> plus a 12 character random suffix, or randomly generated when no display name is given.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Optional. The human-readable display name for the store (at most 512 characters).
    /// </summary>
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    /// <summary>
    /// Output only. The timestamp of when the store was created.
    /// </summary>
    [JsonPropertyName("createTime")]
    public Timestamp? CreateTime { get; set; }

    /// <summary>
    /// Output only. The timestamp of when the store was last updated.
    /// </summary>
    [JsonPropertyName("updateTime")]
    public Timestamp? UpdateTime { get; set; }

    /// <summary>
    /// Output only. The number of documents in the store that are active and ready for retrieval.
    /// </summary>
    [JsonPropertyName("activeDocumentsCount")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long? ActiveDocumentsCount { get; set; }

    /// <summary>
    /// Output only. The number of documents in the store that are being processed.
    /// </summary>
    [JsonPropertyName("pendingDocumentsCount")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long? PendingDocumentsCount { get; set; }

    /// <summary>
    /// Output only. The number of documents in the store that have failed processing.
    /// </summary>
    [JsonPropertyName("failedDocumentsCount")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long? FailedDocumentsCount { get; set; }

    /// <summary>
    /// Output only. The size of raw bytes ingested into the store (total size of all its documents).
    /// </summary>
    [JsonPropertyName("sizeBytes")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long? SizeBytes { get; set; }

    /// <summary>
    /// The embedding model used by the store, in the form <c>models/{model}</c>.
    /// When creating a store, leave unset to use the default embedding model.
    /// </summary>
    [JsonPropertyName("embeddingModel")]
    public string? EmbeddingModel { get; set; }
}
