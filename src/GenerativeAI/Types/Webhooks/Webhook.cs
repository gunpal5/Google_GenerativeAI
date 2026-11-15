using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// A Webhook resource: an endpoint that receives event notifications (for example when a batch job or a
/// background interaction finishes). The Webhooks API uses snake_case field names on the wire.
/// </summary>
/// <seealso href="https://ai.google.dev/gemini-api/docs/webhooks">See Official API Documentation</seealso>
public class Webhook
{
    /// <summary>
    /// Output only. The ID of the webhook.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// Optional. The user-provided name of the webhook.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Required. The URI to which webhook events will be sent.
    /// </summary>
    [JsonPropertyName("uri")]
    public string? Uri { get; set; }

    /// <summary>
    /// Required. The events that the webhook is subscribed to (see <see cref="WebhookEvents"/>).
    /// </summary>
    [JsonPropertyName("subscribed_events")]
    public List<string>? SubscribedEvents { get; set; }

    /// <summary>
    /// Output only. The state of the webhook (see <see cref="WebhookStates"/>).
    /// </summary>
    [JsonPropertyName("state")]
    public string? State { get; set; }

    /// <summary>
    /// Output only. The new signing secret for the webhook. Only populated in the response of a create call;
    /// store it to verify the payloads delivered to <see cref="Uri"/>.
    /// </summary>
    [JsonPropertyName("new_signing_secret")]
    public string? NewSigningSecret { get; set; }

    /// <summary>
    /// Output only. The signing secrets associated with this webhook.
    /// </summary>
    [JsonPropertyName("signing_secrets")]
    public List<SigningSecret>? SigningSecrets { get; set; }

    /// <summary>
    /// Output only. The timestamp (RFC 3339) when the webhook was created.
    /// </summary>
    [JsonPropertyName("create_time")]
    public string? CreateTime { get; set; }

    /// <summary>
    /// Output only. The timestamp (RFC 3339) when the webhook was last updated.
    /// </summary>
    [JsonPropertyName("update_time")]
    public string? UpdateTime { get; set; }
}

/// <summary>
/// Represents a signing secret used to verify webhook payloads.
/// </summary>
public class SigningSecret
{
    /// <summary>
    /// Output only. The truncated version of the signing secret.
    /// </summary>
    [JsonPropertyName("truncated_secret")]
    public string? TruncatedSecret { get; set; }

    /// <summary>
    /// Output only. The expiration timestamp (RFC 3339) of the signing secret.
    /// </summary>
    [JsonPropertyName("expire_time")]
    public string? ExpireTime { get; set; }
}

/// <summary>
/// The request body of <c>POST /webhooks</c> (python-genai <c>WebhookInput</c>).
/// </summary>
public class CreateWebhookRequest
{
    /// <summary>
    /// Optional. The user-provided name of the webhook.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Required. The URI to which webhook events will be sent.
    /// </summary>
    [JsonPropertyName("uri")]
    public string? Uri { get; set; }

    /// <summary>
    /// Required. The events that the webhook is subscribed to (see <see cref="WebhookEvents"/>).
    /// </summary>
    [JsonPropertyName("subscribed_events")]
    public List<string>? SubscribedEvents { get; set; }
}

/// <summary>
/// The request body of <c>PATCH /webhooks/{id}</c> (python-genai <c>WebhookUpdate</c>). Only the fields that are
/// set are sent; combine with an update mask to control which fields are replaced.
/// </summary>
public class WebhookUpdate
{
    /// <summary>
    /// Optional. The user-provided name of the webhook.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Optional. The state of the webhook (see <see cref="WebhookStates"/>), e.g. to disable or re-enable it.
    /// </summary>
    [JsonPropertyName("state")]
    public string? State { get; set; }

    /// <summary>
    /// Optional. The events that the webhook is subscribed to (see <see cref="WebhookEvents"/>).
    /// </summary>
    [JsonPropertyName("subscribed_events")]
    public List<string>? SubscribedEvents { get; set; }

    /// <summary>
    /// Optional. The URI to which webhook events will be sent.
    /// </summary>
    [JsonPropertyName("uri")]
    public string? Uri { get; set; }
}

/// <summary>
/// Response of <c>GET /webhooks</c>.
/// </summary>
public class WebhookListResponse
{
    /// <summary>
    /// The webhooks.
    /// </summary>
    [JsonPropertyName("webhooks")]
    public List<Webhook>? Webhooks { get; set; }

    /// <summary>
    /// A token, which can be sent as <c>page_token</c> to retrieve the next page. If this field is omitted, there
    /// are no subsequent pages.
    /// </summary>
    [JsonPropertyName("next_page_token")]
    public string? NextPageToken { get; set; }
}

/// <summary>
/// Request body of <c>POST /webhooks/{id}:ping</c>. Currently has no fields.
/// </summary>
public class PingWebhookRequest
{
}

/// <summary>
/// Response of <c>POST /webhooks/{id}:ping</c>. Currently has no fields.
/// </summary>
public class WebhookPingResponse
{
}

/// <summary>
/// Request body of <c>POST /webhooks/{id}:rotateSigningSecret</c>.
/// </summary>
public class RotateSigningSecretRequest
{
    /// <summary>
    /// Optional. The revocation behavior for previous signing secrets (see <see cref="WebhookRevocationBehaviors"/>).
    /// </summary>
    [JsonPropertyName("revocation_behavior")]
    public string? RevocationBehavior { get; set; }
}

/// <summary>
/// Response of <c>POST /webhooks/{id}:rotateSigningSecret</c>.
/// </summary>
public class WebhookRotateSigningSecretResponse
{
    /// <summary>
    /// Output only. The newly generated signing secret.
    /// </summary>
    [JsonPropertyName("secret")]
    public string? Secret { get; set; }
}
