using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// A <see cref="Document"/> is a collection of <c>Chunk</c>s. A <c>Corpus</c> can have a maximum of 10,000 <see cref="Document"/>s.
/// </summary>
/// <seealso href="https://ai.google.dev/api/semantic-retrieval/documents#Document">See Official API Documentation</seealso>
public class Document
{
    /// <summary>
    /// Immutable. Identifier. The <see cref="Document"/> resource name. The ID (name excluding the "corpora/*/documents/" prefix) can contain up to 40 characters that are lowercase alphanumeric or dashes (-). The ID cannot start or end with a dash. If the name is empty on create, a unique name will be derived from <see cref="DisplayName"/> along with a 12 character random suffix. Example: <c>corpora/{corpus_id}/documents/my-awesome-doc-123a456b789c</c>
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Optional. The human-readable display name for the <see cref="Document"/>. The display name must be no more than 512 characters in length, including spaces. Example: "Semantic Retriever Documentation"
    /// </summary>
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    /// <summary>
    /// Optional. User provided custom metadata stored as key-value pairs used for querying. A <see cref="Document"/> can have a maximum of 20 <c>CustomMetadata</c>.
    /// </summary>
    [JsonPropertyName("customMetadata")]
    public List<CustomMetadata>? CustomMetadata { get; set; }

    /// <summary>
    /// Output only. The Timestamp of when the <see cref="Document"/> was last updated.
    /// Uses RFC 3339, where generated output will always be Z-normalized and uses 0, 3, 6 or 9 fractional digits. Offsets other than "Z" are also accepted. Examples: <c>"2014-10-02T15:01:23Z"</c>, <c>"2014-10-02T15:01:23.045123456Z"</c> or <c>"2014-10-02T15:01:23+05:30"</c>.
    /// </summary>
    [JsonPropertyName("updateTime")]
    public Timestamp? UpdateTime { get; set; }

    /// <summary>
    /// Output only. The Timestamp of when the <see cref="Document"/> was created.
    /// Uses RFC 3339, where generated output will always be Z-normalized and uses 0, 3, 6 or 9 fractional digits. Offsets other than "Z" are also accepted. Examples: <c>"2014-10-02T15:01:23Z"</c>, <c>"2014-10-02T15:01:23.045123456Z"</c> or <c>"2014-10-02T15:01:23+05:30"</c>.
    /// </summary>
    [JsonPropertyName("createTime")]
    public Timestamp? CreateTime { get; set; }
}