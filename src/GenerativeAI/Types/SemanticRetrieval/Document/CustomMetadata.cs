using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// User provided metadata stored as key-value pairs.
/// </summary>
/// <seealso href="https://ai.google.dev/api/semantic-retrieval/documents#custommetadata">See Official API Documentation</seealso>
public class CustomMetadata
{
    /// <summary>
    /// The key of the metadata to store.
    /// </summary>
    [JsonPropertyName("key")]
    public string Key { get; set; }

    /// <summary>
    /// The string value of the metadata to store.
    /// </summary>
    [JsonPropertyName("stringValue")]
    public string? StringValue { get; set; }

    /// <summary>
    /// The StringList value of the metadata to store.
    /// </summary>
    [JsonPropertyName("stringListValue")]
    public StringList? StringListValue { get; set; }

    /// <summary>
    /// The numeric value of the metadata to store.
    /// </summary>
    [JsonPropertyName("numericValue")]
    public double? NumericValue { get; set; }
}