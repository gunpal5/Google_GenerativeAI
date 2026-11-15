using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// The processing state of a document. This enum is not supported in Vertex AI.
/// </summary>
/// <remarks>Generated from python-genai by scripts/drift/update_dtos.py.</remarks>
[JsonConverter(typeof(JsonStringEnumConverter<DocumentState>))]
public enum DocumentState
{
    /// <summary>
    /// The API value "STATE_UNSPECIFIED".
    /// </summary>
    STATE_UNSPECIFIED = 0,

    /// <summary>
    /// The API value "STATE_PENDING".
    /// </summary>
    STATE_PENDING = 1,

    /// <summary>
    /// The API value "STATE_ACTIVE".
    /// </summary>
    STATE_ACTIVE = 2,

    /// <summary>
    /// The API value "STATE_FAILED".
    /// </summary>
    STATE_FAILED = 3
}
