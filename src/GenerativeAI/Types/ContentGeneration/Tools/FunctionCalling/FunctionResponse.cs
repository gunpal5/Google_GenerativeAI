using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// The result output from a <see cref="FunctionCall"/> that contains a string representing the
/// <see cref="FunctionDeclaration.Name"/> and a structured JSON object containing any output from
/// the function is used as context to the model. This should contain the result of a
/// <see cref="FunctionCall"/> made based on model prediction.
/// </summary>
/// <seealso href="https://ai.google.dev/api/caching#FunctionResponse">See Official API Documentation</seealso>
public class FunctionResponse
{
    /// <summary>
    /// Optional. The id of the function call this response is for. Populated by the client
    /// to match the corresponding function call <c>id</c>.
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
    /// Required. The function response in JSON object format.
    /// </summary>
    [JsonPropertyName("response")]
    public dynamic? Response { get; set; }
}