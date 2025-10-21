using System.Text.Json.Serialization;
using System.Text.Json.Nodes;

namespace GenerativeAI.Types;

/// <summary>
/// Declaration of a function that can be used as a <see cref="Tool">Tool</see>.
/// </summary>
/// <seealso href="https://ai.google.dev/api/caching#FunctionDeclaration">See Official API Documentation</seealso> 
public class FunctionDeclaration
{
    /// <summary>
    /// Required. The name of the function.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    /// <summary>
    /// Optional. Description of the function.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Optional. Defines the parameters accepted by the function.
    /// </summary>
    [JsonPropertyName("parameters")]
    public Schema? Parameters { get; set; }

    /// <summary>
    /// Optional. Defines the function behavior. Defaults to BLOCKING.
    /// </summary>
    [JsonPropertyName("behavior")]
    public Behavior? Behavior { get; set; }

    /// <summary>
    /// Optional. Alternative to parameters using JSON Schema. If set, parameters must be omitted.
    /// </summary>
    [JsonPropertyName("parametersJsonSchema")]
    public JsonNode? ParametersJsonSchema { get; set; }

    /// <summary>
    /// Optional. Defines the expected response schema for the function.
    /// </summary>
    [JsonPropertyName("response")]
    public Schema? Response { get; set; }

    /// <summary>
    /// Optional. Alternative to response using JSON Schema. If set, response must be omitted.
    /// </summary>
    [JsonPropertyName("responseJsonSchema")]
    public JsonNode? ResponseJsonSchema { get; set; }
}