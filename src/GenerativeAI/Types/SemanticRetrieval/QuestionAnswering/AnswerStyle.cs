using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Style for grounded answers.
/// </summary>
/// <seealso href="https://ai.google.dev/api/semantic-retrieval/question-answering#AnswerStyle">See Official API Documentation</seealso>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AnswerStyle
{
    /// <summary>
    /// Unspecified answer style.
    /// </summary>
    ANSWER_STYLE_UNSPECIFIED = 0,

    /// <summary>
    /// Succinct but abstract style.
    /// </summary>
    ABSTRACTIVE = 1,

    /// <summary>
    /// Very brief and extractive style.
    /// </summary>
    EXTRACTIVE = 2,

    /// <summary>
    /// Verbose style including extra details. The response may be formatted as a sentence,
    /// paragraph, multiple paragraphs, or bullet points, etc.
    /// </summary>
    VERBOSE = 3,
}