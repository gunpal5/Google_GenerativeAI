using System.Text.Json;
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
    public JsonNode? Response { get; set; }

    /// <summary>
    /// Optional. Signals that function call continues, and more responses will be returned,
    /// turning the function call into a generator.
    /// </summary>
    [JsonPropertyName("willContinue")]
    public bool? WillContinue { get; set; }

    /// <summary>
    /// Optional. Specifies how the response should be scheduled in the conversation.
    /// Only applicable to NON_BLOCKING function calls. Defaults to WHEN_IDLE.
    /// </summary>
    [JsonPropertyName("scheduling")]
    public FunctionResponseScheduling? Scheduling { get; set; }

    /// <summary>
    /// Optional. List of parts that constitute a function response.
    /// Each part may have a different IANA MIME type.
    /// </summary>
    [JsonPropertyName("parts")]
    public List<FunctionResponsePart>? Parts { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FunctionResponse"/> class with the specified function name.
    /// </summary>
    /// <param name="name">The name of the function this response is for.</param>
    public FunctionResponse(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FunctionResponse"/> class for JSON deserialization.
    /// </summary>
    public FunctionResponse() : this("") { }
}