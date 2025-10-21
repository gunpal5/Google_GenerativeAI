using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Chunk from Google Maps.
/// </summary>
public class GroundingChunkMaps
{
    /// <summary>
    /// Sources used to generate the place answer. This includes review snippets and photos
    /// that were used to generate the answer, as well as URIs to flag content.
    /// </summary>
    [JsonPropertyName("placeAnswerSources")]
    public object? PlaceAnswerSources { get; set; }

    /// <summary>
    /// This Place's resource name, in places/{place_id} format. Can be used to look up the Place.
    /// </summary>
    [JsonPropertyName("placeId")]
    public string? PlaceId { get; set; }

    /// <summary>
    /// Text of the chunk.
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// Title of the chunk.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// URI reference of the chunk.
    /// </summary>
    [JsonPropertyName("uri")]
    public string? Uri { get; set; }
}
