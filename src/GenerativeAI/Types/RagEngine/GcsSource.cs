using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// The Google Cloud Storage location for the input content.
/// </summary>
public class GcsSource
{
    /// <summary>
    /// Required. Google Cloud Storage URI(-s) to the input file(s). May contain wildcards. For more information on wildcards, see https://cloud.google.com/storage/docs/gsutil/addlhelp/WildcardNames.
    /// </summary>
    [JsonPropertyName("uris")]
    public System.Collections.Generic.ICollection<string>? Uris { get; set; } 
}