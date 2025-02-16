using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// A <see cref="Chunk"/> is a subpart of a <see cref="Document"/> that is treated as an independent unit for the purposes of vector representation and storage.
/// A <see cref="Corpus"/> can have a maximum of 1 million <see cref="Chunk"/>s.
/// </summary>
/// <seealso href="https://developers.google.com/semantic-retrieval/chunks#Chunk">See Official API Documentation</seealso>
public class Chunk
{
    /// <summary>
    /// Immutable. Identifier. The <see cref="Chunk"/> resource name. The ID (name excluding the "corpora/*/documents/*/chunks/" prefix) can contain up to 40 characters that are lowercase alphanumeric or dashes (-). The ID cannot start or end with a dash. If the name is empty on create, a random 12-character unique ID will be generated.
    /// Example: <c>corpora/{corpus_id}/documents/{document_id}/chunks/123a456b789c</c>
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// The content for the <see cref="Chunk"/>, such as the text string. The maximum number of tokens per chunk is 2043.
    /// </summary>
    [JsonPropertyName("data")]
    public ChunkData? Data { get; set; }

    /// <summary>
    /// User provided custom metadata stored as key-value pairs. The maximum number of <see cref="CustomMetadata"/> per chunk is 20.
    /// </summary>
    [JsonPropertyName("customMetadata")]
    public List<CustomMetadata>? CustomMetadata { get; set; }

    /// <summary>
    /// Output only. The Timestamp of when the <see cref="Chunk"/> was created.
    /// Uses RFC 3339, where generated output will always be Z-normalized and uses 0, 3, 6 or 9 fractional digits. Offsets other than "Z" are also accepted.
    /// Examples: <c>"2014-10-02T15:01:23Z"</c>, <c>"2014-10-02T15:01:23.045123456Z"</c> or <c>"2014-10-02T15:01:23+05:30"</c>.
    /// </summary>
    [JsonPropertyName("createTime")]
    public Timestamp? CreateTime { get; set; }

    /// <summary>
    /// Output only. The Timestamp of when the <see cref="Chunk"/> was last updated.
    /// Uses RFC 3339, where generated output will always be Z-normalized and uses 0, 3, 6 or 9 fractional digits. Offsets other than "Z" are also accepted.
    /// Examples: <c>"2014-10-02T15:01:23Z"</c>, <c>"2014-10-02T15:01:23.045123456Z"</c> or <c>"2014-10-02T15:01:23+05:30"</c>.
    /// </summary>
    [JsonPropertyName("updateTime")]
    public Timestamp? UpdateTime { get; set; }

    /// <summary>
    /// Output only. Current state of the <see cref="Chunk"/>.
    /// </summary>
    [JsonPropertyName("state")]
    public ChunkState? State { get; set; }
}