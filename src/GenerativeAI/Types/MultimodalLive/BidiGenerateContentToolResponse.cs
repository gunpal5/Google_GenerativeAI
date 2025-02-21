using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Client generated response to a <see cref="BidiGenerateContentToolCall"/> received from the server. Individual <see cref="FunctionResponse"/> objects are matched to the respective <see cref="FunctionCall"/> objects by the <c>id</c> field.
/// Note that in the unary and server-streaming GenerateContent APIs function calling happens by exchanging the <see cref="Content"/> parts, while in the bidi GenerateContent APIs function calling happens over these dedicated set of messages.
/// </summary>
/// <seealso href="https://ai.google.dev/gemini-api/docs/multimodal-live#bidigeneratecontenttoolresponse">See Official API Documentation</seealso>
public class 
    
    BidiGenerateContentToolResponse
{
    /// <summary>
    /// The response to the function calls.
    /// </summary>
    [JsonPropertyName("functionResponses")]
    public FunctionResponse[]? FunctionResponses { get; set; }
}