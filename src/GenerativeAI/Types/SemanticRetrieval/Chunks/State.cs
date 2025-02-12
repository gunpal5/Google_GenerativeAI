using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// States for the lifecycle of a <see cref="Chunk"/>.
/// </summary>
/// <seealso href="https://ai.google.dev/api/semantic-retrieval/chunks#State">See Official API Documentation</seealso>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChunkState
{
    /// <summary>
    /// The default value. This value is used if the state is omitted.
    /// </summary>
    STATE_UNSPECIFIED,

    /// <summary>
    /// <see cref="Chunk"/> is being processed (embedding and vector storage).
    /// </summary>
    STATE_PENDING_PROCESSING,

    /// <summary>
    /// <see cref="Chunk"/> is processed and available for querying.
    /// </summary>
    STATE_ACTIVE,

    /// <summary>
    /// <see cref="Chunk"/> failed processing.
    /// </summary>
    STATE_FAILED
}