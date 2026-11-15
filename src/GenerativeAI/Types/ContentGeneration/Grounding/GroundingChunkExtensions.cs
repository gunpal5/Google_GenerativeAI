using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// An image used to ground the response (from Google Search image search).
/// </summary>
public class GroundingChunkImage
{
    /// <summary>
    /// URI of the page the image was found on.
    /// </summary>
    [JsonPropertyName("sourceUri")]
    public string? SourceUri { get; set; }

    /// <summary>
    /// URI of the image itself.
    /// </summary>
    [JsonPropertyName("imageUri")]
    public string? ImageUri { get; set; }

    /// <summary>
    /// Title of the source page.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// Domain of the source page.
    /// </summary>
    [JsonPropertyName("domain")]
    public string? Domain { get; set; }
}

/// <summary>
/// A route returned by Google Maps grounding.
/// </summary>
public class GroundingChunkMapsRoute
{
    /// <summary>
    /// Length of the route in meters.
    /// </summary>
    [JsonPropertyName("distanceMeters")]
    public int? DistanceMeters { get; set; }

    /// <summary>
    /// Travel time, as a duration string (for example "1800s").
    /// </summary>
    [JsonPropertyName("duration")]
    public string? Duration { get; set; }

    /// <summary>
    /// The route as an encoded polyline.
    /// </summary>
    [JsonPropertyName("encodedPolyline")]
    public string? EncodedPolyline { get; set; }
}

/// <summary>
/// A custom metadata entry attached to a File Search document.
/// </summary>
public class GroundingChunkCustomMetadata
{
    /// <summary>
    /// The metadata key.
    /// </summary>
    [JsonPropertyName("key")]
    public string? Key { get; set; }

    /// <summary>
    /// The value, when it is a string.
    /// </summary>
    [JsonPropertyName("stringValue")]
    public string? StringValue { get; set; }

    /// <summary>
    /// The value, when it is a list of strings.
    /// </summary>
    [JsonPropertyName("stringListValue")]
    public GroundingChunkStringList? StringListValue { get; set; }

    /// <summary>
    /// The value, when it is a number.
    /// </summary>
    [JsonPropertyName("numericValue")]
    public double? NumericValue { get; set; }
}

/// <summary>
/// A list of string values in custom metadata.
/// </summary>
public class GroundingChunkStringList
{
    /// <summary>
    /// The values.
    /// </summary>
    [JsonPropertyName("values")]
    public List<string>? Values { get; set; }
}
