using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Content that has been preprocessed and can be used in subsequent requests to GenerativeService.
/// Cached content can be only used with the model it was created for.
/// </summary>
/// <seealso href="https://ai.google.dev/api/caching#CachedContent">See Official API Documentation</seealso> 
public class CachedContent
{
    /// <summary>
    /// Optional. Input only. Immutable. The content to cache.
    /// </summary>
    [JsonPropertyName("contents")]
    public List<Content>? Contents { get; set; }

    /// <summary>
    /// Optional. Input only. Immutable. A list of <see cref="Tool"/>s the model may use to generate the next response.
    /// </summary>
    [JsonPropertyName("tools")]
    public List<Tool>? Tools { get; set; }

    /// <summary>
    /// Output only. Creation time of the cache entry.
    /// Uses RFC 3339, where generated output will always be Z-normalized and uses 0, 3, 6 or 9 fractional
    /// digits. Offsets other than "Z" are also accepted.
    /// Examples: <c>"2014-10-02T15:01:23Z"</c>, <c>"2014-10-02T15:01:23.045123456Z"</c> or <c>"2014-10-02T15:01:23+05:30"</c>.
    /// </summary>
    [JsonPropertyName("createTime")]
    public Timestamp? CreateTime { get; set; }

    /// <summary>
    /// Output only. When the cache entry was last updated in UTC time.
    /// Uses RFC 3339, where generated output will always be Z-normalized and uses 0, 3, 6 or 9 fractional
    /// digits. Offsets other than "Z" are also accepted.
    /// Examples: <c>"2014-10-02T15:01:23Z"</c>, <c>"2014-10-02T15:01:23.045123456Z"</c> or <c>"2014-10-02T15:01:23+05:30"</c>.
    /// </summary>
    [JsonPropertyName("updateTime")]
    public Timestamp? UpdateTime { get; set; }

    /// <summary>
    /// Output only. Metadata on the usage of the cached content.
    /// </summary>
    [JsonPropertyName("usageMetadata")]
    public UsageMetadata? UsageMetadata { get; set; }

    /// <summary>
    /// Timestamp in UTC of when this resource is considered expired. This is *always* provided on output,
    /// regardless of what was sent on input.
    /// Uses RFC 3339, where generated output will always be Z-normalized and uses 0, 3, 6 or 9 fractional
    /// digits. Offsets other than "Z" are also accepted.
    /// Examples: <c>"2014-10-02T15:01:23Z"</c>, <c>"2014-10-02T15:01:23.045123456Z"</c> or <c>"2014-10-02T15:01:23+05:30"</c>.
    /// </summary>
    [JsonPropertyName("expireTime")]
    public Timestamp? ExpireTime { get; set; }

    /// <summary>
    /// Input only. New TTL for this resource, input only.
    /// A duration in seconds with up to nine fractional digits, ending with '<c>s</c>'. Example: <c>"3.5s"</c>.
    /// </summary>
    [JsonPropertyName("ttl")]
    public Duration? Ttl { get; set; }

    /// <summary>
    /// Optional. Identifier. The resource name referring to the cached content. Format: <c>cachedContents/{id}</c>
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Optional. Immutable. The user-generated meaningful display name of the cached content.
    /// Maximum 128 Unicode characters.
    /// </summary>
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    /// <summary>
    /// Required. Immutable. The name of the <c>Model</c> to use for cached content.
    /// Format: <c>models/{model}</c>
    /// </summary>
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>
    /// Optional. Input only. Immutable. Developer set system instruction. Currently text only.
    /// </summary>
    [JsonPropertyName("systemInstruction")]
    public Content? SystemInstruction { get; set; }

    /// <summary>
    /// Optional. Input only. Immutable. Tool config. This config is shared for all tools.
    /// </summary>
    [JsonPropertyName("toolConfig")]
    public ToolConfig? ToolConfig { get; set; }
}