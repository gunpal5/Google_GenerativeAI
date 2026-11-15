using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Per-request webhook configuration for long-running operations (e.g. batch jobs): notifications are sent when
/// the operation completes. Only supported by the Gemini Developer API.
/// </summary>
public class WebhookConfig
{
    /// <summary>
    /// The webhook URIs to receive notifications. If set, these webhook URIs are used instead of the registered
    /// webhooks.
    /// </summary>
    [JsonPropertyName("uris")]
    public List<string>? Uris { get; set; }

    /// <summary>
    /// User metadata that will be included in each webhook event notification. Use this to attach custom
    /// key-value data to correlate webhook events with your internal systems.
    /// </summary>
    [JsonPropertyName("userMetadata")]
    public Dictionary<string, JsonNode?>? UserMetadata { get; set; }
}
