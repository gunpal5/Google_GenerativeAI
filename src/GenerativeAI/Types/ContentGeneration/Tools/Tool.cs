using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Tool details that the model may use to generate response.
/// A <see cref="Tool">Tool</see> is a piece of code that enables the system to interact with external systems
/// to perform an action, or set of actions, outside of knowledge and scope of the model.
/// </summary>
/// <seealso href="https://ai.google.dev/api/caching#Tool">See Official API Documentation</seealso> 
public class Tool
{
    /// <summary>
    /// Optional. A list of FunctionDeclarations available to the model that can be used for function calling.
    /// The model or system does not execute the function. Instead the defined function may be returned
    /// as a <see cref="Part.FunctionCall"/> with arguments to the client side for execution.
    /// The model may decide to call a subset of these functions by populating
    /// <see cref="Part.FunctionCall"/> in the response. The next conversation turn may contain
    /// a <see cref="Part.FunctionResponse"/> with the <see cref="Content.Role"/> "function"
    /// generation context for the next model turn.
    /// </summary>
    [JsonPropertyName("functionDeclarations")]
    public List<FunctionDeclaration>? FunctionDeclarations { get; set; }

    /// <summary>
    /// Optional. Retrieval tool that is powered by Google search.
    /// </summary>
    [JsonPropertyName("googleSearchRetrieval")]
    public GoogleSearchRetrievalTool? GoogleSearchRetrieval { get; set; }

    /// <summary>
    /// Optional. Enables the model to execute code as part of generation.
    /// </summary>
    [JsonPropertyName("codeExecution")]
    public CodeExecutionTool? CodeExecution { get; set; }

    /// <summary>
    /// Optional. GoogleSearch tool type. Tool to support Google Search in Model. Powered by Google.
    /// </summary>
    [JsonPropertyName("googleSearch")]
    public GoogleSearchTool? GoogleSearch { get; set; }
    
    /// <summary>
    /// Optional. Retrieval tool type. System will always execute the provided retrieval tool(s) to get external knowledge to answer the prompt. Retrieval results are presented to the model for generation.
    /// </summary>
    [JsonPropertyName("retrieval")]
    public VertexRetrievalTool? Retrieval { get; set; }
}