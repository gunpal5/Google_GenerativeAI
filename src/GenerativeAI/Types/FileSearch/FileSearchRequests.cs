using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Optional settings for uploading a file directly into a <see cref="FileSearchStore"/>.
/// Sent as the JSON metadata of the <c>uploadToFileSearchStore</c> request.
/// </summary>
/// <seealso href="https://ai.google.dev/api/file-search/file-search-stores#method:-filesearchstores.uploadtofilesearchstore">See Official API Documentation</seealso>
public class UploadToFileSearchStoreConfig
{
    /// <summary>
    /// MIME type of the uploaded data. When uploading a local file and this is not set,
    /// it is inferred from the file extension.
    /// </summary>
    [JsonPropertyName("mimeType")]
    public string? MimeType { get; set; }

    /// <summary>
    /// Display name of the created document.
    /// </summary>
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    /// <summary>
    /// User provided custom metadata stored as key-value pairs, usable for filtering at query time.
    /// </summary>
    [JsonPropertyName("customMetadata")]
    public List<CustomMetadata>? CustomMetadata { get; set; }

    /// <summary>
    /// How the service should chunk the file.
    /// </summary>
    [JsonPropertyName("chunkingConfig")]
    public ChunkingConfig? ChunkingConfig { get; set; }
}

/// <summary>
/// Optional settings for importing an existing Files API file into a <see cref="FileSearchStore"/>.
/// </summary>
/// <seealso href="https://ai.google.dev/api/file-search/file-search-stores#method:-filesearchstores.importfile">See Official API Documentation</seealso>
public class ImportFileConfig
{
    /// <summary>
    /// User provided custom metadata stored as key-value pairs, usable for filtering at query time.
    /// </summary>
    [JsonPropertyName("customMetadata")]
    public List<CustomMetadata>? CustomMetadata { get; set; }

    /// <summary>
    /// How the service should chunk the file.
    /// </summary>
    [JsonPropertyName("chunkingConfig")]
    public ChunkingConfig? ChunkingConfig { get; set; }
}

/// <summary>
/// Request body for <c>fileSearchStores.importFile</c>.
/// </summary>
public class ImportFileRequest : ImportFileConfig
{
    /// <summary>
    /// Required. The resource name of the Files API file to import, for example <c>files/abc-123</c>.
    /// </summary>
    [JsonPropertyName("fileName")]
    public string? FileName { get; set; }
}
