using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// User provided filter to limit retrieval based on <see cref="Chunk"/> or <see cref="Document"/> level metadata
/// values. Example (genre = drama OR genre = action):
/// key = "document.custom_metadata.genre"
/// conditions = [{stringValue = "drama", operation = EQUAL}, {stringValue = "action", operation = EQUAL}]
/// </summary>
/// <seealso href="https://ai.google.dev/api/semantic-retrieval/corpora#metadatafilter">See Official API Documentation</seealso>
public class MetadataFilter
{
    /// <summary>
    /// Required. The key of the metadata to filter on.
    /// </summary>
    [JsonPropertyName("key")]
    public string Key { get; set; } = "";

    /// <summary>
    /// Required. The <see cref="Condition"/>s for the given key that will trigger this filter.
    /// Multiple <see cref="Condition"/>s are joined by logical ORs.
    /// </summary>
    [JsonPropertyName("conditions")]
    public List<Condition> Conditions { get; set; } = new List<Condition>();
}