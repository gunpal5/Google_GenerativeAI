using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Source-generated JSON metadata for the Webhooks API types. <see cref="GenerativeAI.Clients.WebhooksClient"/>
/// puts it first in its resolver chain, so the Webhooks types also work with Native AOT and trimming.
/// </summary>
[JsonSourceGenerationOptions(WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, UseStringEnumConverter = true)]
[JsonSerializable(typeof(Webhook))]
[JsonSerializable(typeof(SigningSecret))]
[JsonSerializable(typeof(CreateWebhookRequest))]
[JsonSerializable(typeof(WebhookUpdate))]
[JsonSerializable(typeof(WebhookListResponse))]
[JsonSerializable(typeof(PingWebhookRequest))]
[JsonSerializable(typeof(WebhookPingResponse))]
[JsonSerializable(typeof(RotateSigningSecretRequest))]
[JsonSerializable(typeof(WebhookRotateSigningSecretResponse))]
[JsonSerializable(typeof(WebhookConfig))]
public partial class WebhooksSerializerContext : JsonSerializerContext
{
}
