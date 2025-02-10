using System.Text.Json.Serialization;
using GenerativeAI.Types.ContentGeneration.Tools.FunctionCalling;

namespace GenerativeAI.Types.ContentGeneration.Tools;

/// <summary>
/// The Tool configuration containing parameters for specifying <see cref="Tool">Tool</see> use in the request.
/// </summary>
/// <seealso href="https://ai.google.dev/api/rest/v1beta/ToolConfig">See Official API Documentation</seealso> 
public class ToolConfig
{
    /// <summary>
    /// Optional. Function calling config.
    /// </summary>
    [JsonPropertyName("functionCallingConfig")]
    public FunctionCallingConfig? FunctionCallingConfig { get; set; }
}