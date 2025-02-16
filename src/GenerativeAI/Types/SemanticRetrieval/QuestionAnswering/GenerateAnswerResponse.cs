using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Response from the model for a grounded answer.
/// </summary>
/// <seealso href="https://ai.google.dev/api/semantic-retrieval/question-answering#response-body">See Official API Documentation</seealso>
public class GenerateAnswerResponse
{
    /// <summary>
    /// Candidate answer from the model.
    /// Note: The model *always* attempts to provide a grounded answer, even when the answer is unlikely to be
    /// answerable from the given passages. In that case, a low-quality or ungrounded answer may be provided,
    /// along with a low <see cref="AnswerableProbability"/>.
    /// </summary>
    [JsonPropertyName("answer")]
    public Candidate? Answer { get; set; }

    /// <summary>
    /// Output only. The model's estimate of the probability that its answer is correct and grounded in the
    /// input passages.
    /// A low <c>answerableProbability</c> indicates that the answer might not be grounded in the sources.
    /// When <c>answerableProbability</c> is low, you may want to:
    /// - Display a message to the effect of "We couldn’t answer that question" to the user.
    /// - Fall back to a general-purpose LLM that answers the question from world knowledge. The threshold and
    ///   nature of such fallbacks will depend on individual use cases. <c>0.5</c> is a good starting threshold.
    /// </summary>
    [JsonPropertyName("answerableProbability")]
    public double? AnswerableProbability { get; set; }

    /// <summary>
    /// Output only. Feedback related to the input data used to answer the question, as opposed to the
    /// model-generated response to the question.
    /// The input data can be one or more of the following:
    /// - Question specified by the last entry in <see cref="GenerateAnswerRequest.Contents"/>
    /// - Conversation history specified by the other entries in <see cref="GenerateAnswerRequest.Contents"/>
    /// - Grounding sources (<see cref="GroundingSource.SemanticRetriever"/> or
    ///   <see cref="GroundingSource.InlinePassages"/>)
    /// </summary>
    [JsonPropertyName("inputFeedback")]
    public InputFeedback? InputFeedback { get; set; }
}