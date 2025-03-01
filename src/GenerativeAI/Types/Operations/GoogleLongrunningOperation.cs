using System.Text.Json.Serialization;
using GenerativeAI.Types.RagEngine;

namespace GenerativeAI.Types;

/// <summary>
/// This resource represents a long-running operation that is the result of a network API call.
/// </summary>
public class GoogleLongRunningOperation
{
    /// <summary>
    /// If the value is `false`, it means the operation is still in progress. If `true`, the operation is completed, and either `error` or `response` is available.
    /// </summary>
    [JsonPropertyName("done")]
    public bool? Done { get; set; } 

    /// <summary>
    /// The error result of the operation in case of failure or cancellation.
    /// </summary>
    [JsonPropertyName("error")]
    public GoogleRpcStatus? Error { get; set; } 

    /// <summary>
    /// Service-specific metadata associated with the operation. It typically contains progress information and common metadata such as create time. Some services might not provide such metadata. Any method that returns a long-running operation should document the metadata type, if any.
    /// </summary>
    [JsonPropertyName("metadata")]
    public System.Collections.Generic.IDictionary<string, object>? Metadata { get; set; } 

    /// <summary>
    /// The server-assigned name, which is only unique within the same service that originally returns it. If you use the default HTTP mapping, the `name` should be a resource name ending with `operations/{unique_id}`.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; } 

    /// <summary>
    /// The normal, successful response of the operation. If the original method returns no data on success, such as `Delete`, the response is `google.protobuf.Empty`. If the original method is standard `Get`/`Create`/`Update`, the response should be the resource. For other methods, the response should have the type `XxxResponse`, where `Xxx` is the original method name. For example, if the original method name is `TakeSnapshot()`, the inferred response type is `TakeSnapshotResponse`.
    /// </summary>
    [JsonPropertyName("response")]
    public System.Collections.Generic.IDictionary<string, object>? Response { get; set; } 
}