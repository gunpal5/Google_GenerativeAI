using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Message sent by the server to indicate that the current connection should be terminated
/// and the client should cease sending further requests on this stream.
/// This is often used for graceful shutdown or when the server is no longer able to
/// process requests on the current stream.
/// </summary>
public class LiveServerGoAway
{
    /// <summary>
    /// An optional error code that explains why the server is terminating the connection.
    /// This could correspond to standard gRPC status codes or custom application-specific codes.
    /// </summary>
    [JsonPropertyName("errorCode")]
    public int? ErrorCode { get; set; }

    /// <summary>
    /// An optional human-readable message providing more details about why the server is going away.
    /// </summary>
    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Optional. Indicates if the client should attempt to reconnect.
    /// If true, the client may attempt to establish a new connection.
    /// </summary>
    [JsonPropertyName("reconnect")]
    public bool? Reconnect { get; set; }

    /// <summary>
    /// Optional. The suggested delay in seconds before the client attempts to reconnect, if 'reconnect' is true.
    /// </summary>
    [JsonPropertyName("retryAfterSeconds")]
    public int? RetryAfterSeconds { get; set; }
}