using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// A predicted <see cref="FunctionCall"/> returned from the model that contains a string representing
/// the <see cref="FunctionDeclaration.Name"/> with the arguments and their values.
/// </summary>
/// <seealso href="https://ai.google.dev/api/caching#FunctionCall">See Official API Documentation</seealso> 
public class FunctionCall
{
    /// <summary>
    /// Optional. The unique id of the function call. If populated, the client to execute the
    /// <see cref="FunctionCall"/> and return the response with the matching <see cref="Id"/>.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// Required. The name of the function to call. Must be a-z, A-Z, 0-9, or contain
    /// underscores and dashes, with a maximum length of 63.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// Optional. The function parameters and values in JSON object format.
    /// </summary>
    [JsonPropertyName("args")]
    public object? Args { get; set; }  
}