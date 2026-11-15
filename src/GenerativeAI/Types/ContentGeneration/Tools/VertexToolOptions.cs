using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Confidence threshold for blocking phishing sites in search results. Vertex AI only.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<PhishBlockThreshold>))]
public enum PhishBlockThreshold
{
    /// <summary>The threshold is unspecified.</summary>
    PHISH_BLOCK_THRESHOLD_UNSPECIFIED,
    /// <summary>Block sites with low or higher phishing confidence.</summary>
    BLOCK_LOW_AND_ABOVE,
    /// <summary>Block sites with medium or higher phishing confidence.</summary>
    BLOCK_MEDIUM_AND_ABOVE,
    /// <summary>Block sites with high or higher phishing confidence.</summary>
    BLOCK_HIGH_AND_ABOVE,
    /// <summary>Block sites with higher or higher phishing confidence.</summary>
    BLOCK_HIGHER_AND_ABOVE,
    /// <summary>Block sites with very high or higher phishing confidence.</summary>
    BLOCK_VERY_HIGH_AND_ABOVE,
    /// <summary>Block only sites with extremely high phishing confidence.</summary>
    BLOCK_ONLY_EXTREMELY_HIGH
}

/// <summary>
/// Whether images of prominent people may be generated. Vertex AI only.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<ProminentPeople>))]
public enum ProminentPeople
{
    /// <summary>Unspecified.</summary>
    PROMINENT_PEOPLE_UNSPECIFIED,
    /// <summary>Allow generating images of prominent people.</summary>
    ALLOW_PROMINENT_PEOPLE,
    /// <summary>Block generating images of prominent people.</summary>
    BLOCK_PROMINENT_PEOPLE
}

/// <summary>
/// A streamed piece of a function call's arguments. Vertex AI only; enabled with
/// <see cref="FunctionCallingConfig.StreamFunctionCallArguments"/>.
/// </summary>
public class PartialArg
{
    /// <summary>
    /// JSON path of the argument this value belongs to, for example <c>$.location</c>.
    /// </summary>
    [JsonPropertyName("jsonPath")]
    public string? JsonPath { get; set; }

    /// <summary>
    /// The value, when it is a string (or a chunk of a string).
    /// </summary>
    [JsonPropertyName("stringValue")]
    public string? StringValue { get; set; }

    /// <summary>
    /// The value, when it is a number.
    /// </summary>
    [JsonPropertyName("numberValue")]
    public double? NumberValue { get; set; }

    /// <summary>
    /// The value, when it is a boolean.
    /// </summary>
    [JsonPropertyName("boolValue")]
    public bool? BoolValue { get; set; }

    /// <summary>
    /// Set to <c>NULL_VALUE</c> when the value is JSON null.
    /// </summary>
    [JsonPropertyName("nullValue")]
    public string? NullValue { get; set; }

    /// <summary>
    /// True if more chunks of this argument will follow.
    /// </summary>
    [JsonPropertyName("willContinue")]
    public bool? WillContinue { get; set; }
}

/// <summary>
/// The kinds of Google Maps grounding to use. Vertex AI only.
/// </summary>
public class GoogleMapsGroundingTypes
{
    /// <summary>
    /// Enables grounding in places.
    /// </summary>
    [JsonPropertyName("places")]
    public GoogleMapsPlaces? Places { get; set; }

    /// <summary>
    /// Enables grounding in routes.
    /// </summary>
    [JsonPropertyName("routing")]
    public GoogleMapsRouting? Routing { get; set; }
}

/// <summary>
/// Places grounding settings. Currently has no options; set an instance to enable it.
/// </summary>
public class GoogleMapsPlaces
{
}

/// <summary>
/// Routing grounding settings. Currently has no options; set an instance to enable it.
/// </summary>
public class GoogleMapsRouting
{
}

/// <summary>
/// Parallel AI web search tool. Vertex AI only.
/// </summary>
public class ToolParallelAiSearch
{
    /// <summary>
    /// Parallel AI API key.
    /// </summary>
    [JsonPropertyName("apiKey")]
    public string? ApiKey { get; set; }

    /// <summary>
    /// Provider-specific search options.
    /// </summary>
    [JsonPropertyName("customConfigs")]
    public JsonObject? CustomConfigs { get; set; }

    /// <summary>
    /// Whether the provider may retain data.
    /// </summary>
    [JsonPropertyName("enableDataRetention")]
    public bool? EnableDataRetention { get; set; }

    /// <summary>
    /// Whether to require zero data retention by the provider.
    /// </summary>
    [JsonPropertyName("enableZeroDataRetention")]
    public bool? EnableZeroDataRetention { get; set; }
}

/// <summary>
/// Exa AI web search tool. Vertex AI only.
/// </summary>
public class ToolExaAiSearch
{
    /// <summary>
    /// Exa API key.
    /// </summary>
    [JsonPropertyName("apiKey")]
    public string? ApiKey { get; set; }

    /// <summary>
    /// Provider-specific search options.
    /// </summary>
    [JsonPropertyName("customConfigs")]
    public JsonObject? CustomConfigs { get; set; }
}
