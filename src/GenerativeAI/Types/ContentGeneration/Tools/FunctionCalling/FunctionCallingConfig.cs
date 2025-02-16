using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Configuration for specifying function calling behavior.
/// </summary>
/// <seealso href="https://ai.google.dev/api/rest/v1beta/FunctionCallingConfig">See Official API Documentation</seealso> 
public class FunctionCallingConfig
{
    /// <summary>
    /// Optional. Specifies the mode in which function calling should execute.
    /// If unspecified, the default value will be set to AUTO.
    /// </summary>
    [JsonPropertyName("mode")]
    public FunctionCallingMode? Mode { get; set; } // Nullable; defaults to AUTO if not specified

    /// <summary>
    /// Optional. A set of function names that, when provided, limits the functions the model will call.
    /// This should only be set when the Mode is ANY. Function names should match [FunctionDeclaration.name].
    /// With mode set to ANY, model will predict a function call from the set of function names provided.
    /// </summary>
    [JsonPropertyName("allowedFunctionNames")]
    public List<string>? AllowedFunctionNames { get; set; }
}