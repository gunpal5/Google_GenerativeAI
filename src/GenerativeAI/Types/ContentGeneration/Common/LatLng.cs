using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// An object that represents a latitude/longitude pair.
/// This is expressed as a pair of doubles to represent degrees latitude and degrees longitude.
/// Unless specified otherwise, this object must conform to the WGS84 standard.
/// Values must be within normalized ranges.
/// </summary>
public class LatLng
{
    /// <summary>
    /// The latitude in degrees. It must be in the range [-90.0, +90.0].
    /// </summary>
    [JsonPropertyName("latitude")]
    public double? Latitude { get; set; }

    /// <summary>
    /// The longitude in degrees. It must be in the range [-180.0, +180.0].
    /// </summary>
    [JsonPropertyName("longitude")]
    public double? Longitude { get; set; }
}
