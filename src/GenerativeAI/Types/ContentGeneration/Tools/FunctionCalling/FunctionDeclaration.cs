using System.Text.Json.Serialization;

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
}