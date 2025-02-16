using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Filter condition applicable to a single key.
/// </summary>
/// <seealso href="https://ai.google.dev/api/semantic-retrieval/corpora#Condition">See Official API Documentation</seealso>
public class Condition
{
    /// <summary>
    /// Required. Operator applied to the given key-value pair to trigger the condition.
    /// </summary>
    [JsonPropertyName("operation")]
    public Operator Operation { get; set; }

    /// <summary>
    /// The string value to filter the metadata on.
    /// </summary>
    [JsonPropertyName("stringValue")]
    public string? StringValue { get; set; }

    /// <summary>
    /// The numeric value to filter the metadata on.
    /// </summary>
    [JsonPropertyName("numericValue")]
    public double? NumericValue { get; set; }
}