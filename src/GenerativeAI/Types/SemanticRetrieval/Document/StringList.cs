using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// User provided string values assigned to a single metadata key.
/// </summary>
/// <seealso href="https://ai.google.dev/api/semantic-retrieval/documents#StringList">See Official API Documentation</seealso>
public class StringList
{
    /// <summary>
    /// The string values of the metadata to store.
    /// </summary>
    [JsonPropertyName("values")]
    public string[]? Values { get; set; }
}