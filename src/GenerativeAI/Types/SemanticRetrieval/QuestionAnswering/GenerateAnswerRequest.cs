using System.Text.Json.Serialization;
using GenerativeAI.Core;

namespace GenerativeAI.Types;



/// <summary>
/// Request to generate an answer.
/// </summary>
/// <seealso href="https://ai.google.dev/api/semantic-retrieval/question-answering#request-body">See Official API Documentation</seealso>
public class GenerateAnswerRequest:IContentsRequest
{
    /// <summary>
    /// Required. The content of the current conversation with the <see cref="Model"/>.
    /// For single-turn queries, this is a single question to answer. For multi-turn queries,
    /// this is a repeated field that contains conversation history and the last <see cref="Content"/>
    /// in the list containing the question.
    /// Note: <c>models.generateAnswer</c> only supports queries in English.
    /// </summary>
    [JsonPropertyName("contents")]
    public List<Content> Contents { get; set; } = new List<Content>();

    /// <summary>
    /// Required. Style in which answers should be returned.
    /// </summary>
    [JsonPropertyName("answerStyle")]
    public AnswerStyle AnswerStyle { get; set; }

    /// <summary>
    /// Optional. A list of unique <see cref="SafetySetting"/> instances for blocking unsafe content.
    /// This will be enforced on the <see cref="GenerateAnswerRequest.Contents"/> and
    /// <c>GenerateAnswerResponse.candidate</c>. There should not be more than one setting for each
    /// <see cref="HarmCategory">SafetyCategory</see> type. The API will block any contents and responses that fail to meet
    /// the thresholds set by these settings. This list overrides the default settings for each
    /// <see cref="HarmCategory">SafetyCategory</see> specified in the safetySettings. If there is no <see cref="SafetySetting"/>
    /// for a given <see cref="HarmCategory">SafetyCategory</see> provided in the list, the API will use the default safety
    /// setting for that category. Harm categories HARM_CATEGORY_HATE_SPEECH,
    /// HARM_CATEGORY_SEXUALLY_EXPLICIT, HARM_CATEGORY_DANGEROUS_CONTENT, HARM_CATEGORY_HARASSMENT
    /// are supported. Refer to the
    /// <see href="https://ai.google.dev/gemini-api/docs/safety-settings">guide</see>
    /// for detailed information on available safety settings. Also refer to the
    /// <see href="https://ai.google.dev/gemini-api/docs/safety-guidance">Safety guidance</see>
    /// to learn how to incorporate safety considerations in your AI applications.
    /// </summary>
    [JsonPropertyName("safetySettings")]
    public List<SafetySetting>? SafetySettings { get; set; }

    // /// <summary>
    // /// The sources in which to ground the answer.
    // /// </summary>
    // [JsonPropertyName("groundingSource")]
    // public GroundingSource? GroundingSource { get; set; }
    
    /// <summary>
    /// Passages provided inline with the request.
    /// </summary>
    [JsonPropertyName("inlinePassages")]
    public GroundingPassages? InlinePassages { get; set; }

    /// <summary>
    /// Content retrieved from resources created via the Semantic Retriever API.
    /// </summary>
    [JsonPropertyName("semanticRetriever")]
    public SemanticRetrieverConfig? SemanticRetriever { get; set; }

    /// <summary>
    /// Optional. Controls the randomness of the output.
    /// Values can range from, inclusive. A value closer to 1.0 will produce responses
    /// that are more varied and creative, while a value closer to 0.0 will typically result in
    /// more straightforward responses from the model. A low temperature (~0.2) is usually
    /// recommended for Attributed-Question-Answering use cases.
    /// </summary>
    [JsonPropertyName("temperature")]
    public double? Temperature { get; set; }
}