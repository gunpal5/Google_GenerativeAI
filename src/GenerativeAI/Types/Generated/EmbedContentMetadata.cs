using System.Text.Json;
using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Request-level metadata for the Gemini Enterprise Agent Platform embed content API.
/// </summary>
/// <remarks>Generated from python-genai by scripts/drift/update_dtos.py.</remarks>
public class EmbedContentMetadata
{
    /// <summary>
    /// Gemini Enterprise Agent Platform only. The total number of billable characters in the request.
    /// </summary>
    [JsonPropertyName("billableCharacterCount")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long? BillableCharacterCount { get; set; }
}
