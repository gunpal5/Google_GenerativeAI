using System.Text.Json;
using System.Text.Json.Serialization;
using GenerativeAI.Types.RagEngine;

namespace GenerativeAI.Types;

/// <summary>
/// Response of <c>fileSearchStores.list</c>.
/// </summary>
public class ListFileSearchStoresResponse
{
    /// <summary>
    /// The returned File Search stores.
    /// </summary>
    [JsonPropertyName("fileSearchStores")]
    public List<FileSearchStore>? FileSearchStores { get; set; }

    /// <summary>
    /// A token that can be sent as <c>pageToken</c> to retrieve the next page. Omitted when there are no more pages.
    /// </summary>
    [JsonPropertyName("nextPageToken")]
    public string? NextPageToken { get; set; }
}

/// <summary>
/// Response of <c>fileSearchStores.documents.list</c>.
/// </summary>
public class ListFileSearchDocumentsResponse
{
    /// <summary>
    /// The returned documents.
    /// </summary>
    [JsonPropertyName("documents")]
    public List<FileSearchDocument>? Documents { get; set; }

    /// <summary>
    /// A token that can be sent as <c>pageToken</c> to retrieve the next page. Omitted when there are no more pages.
    /// </summary>
    [JsonPropertyName("nextPageToken")]
    public string? NextPageToken { get; set; }
}

/// <summary>
/// Long-running operation returned when a file is uploaded to (<c>uploadToFileSearchStore</c>) or
/// imported into (<c>importFile</c>) a <see cref="FileSearchStore"/>.
/// </summary>
public class FileSearchOperation
{
    /// <summary>
    /// The server-assigned operation name, for example
    /// <c>fileSearchStores/{store}/operations/{id}</c> or <c>fileSearchStores/{store}/upload/operations/{id}</c>.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Service-specific metadata associated with the operation.
    /// </summary>
    [JsonPropertyName("metadata")]
    public JsonElement? Metadata { get; set; }

    /// <summary>
    /// <c>false</c> (or unset) while the operation is still in progress; <c>true</c> once it has completed,
    /// in which case either <see cref="Error"/> or <see cref="Response"/> is set.
    /// </summary>
    [JsonPropertyName("done")]
    public bool? Done { get; set; }

    /// <summary>
    /// The error result of the operation in case of failure or cancellation.
    /// </summary>
    [JsonPropertyName("error")]
    public GoogleRpcStatus? Error { get; set; }

    /// <summary>
    /// The result of the operation, available when it completed successfully.
    /// </summary>
    [JsonPropertyName("response")]
    public FileSearchOperationResponse? Response { get; set; }
}

/// <summary>
/// The result of a completed upload or import into a <see cref="FileSearchStore"/>.
/// </summary>
public class FileSearchOperationResponse
{
    /// <summary>
    /// The protobuf type URL of the response message.
    /// </summary>
    [JsonPropertyName("@type")]
    public string? Type { get; set; }

    /// <summary>
    /// The name of the File Search store containing the document.
    /// </summary>
    [JsonPropertyName("parent")]
    public string? Parent { get; set; }

    /// <summary>
    /// The resource name of the created document.
    /// </summary>
    [JsonPropertyName("documentName")]
    public string? DocumentName { get; set; }
}
