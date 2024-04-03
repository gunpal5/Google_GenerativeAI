using GenerativeAI.Tools;

namespace GenerativeAI.Types
{
    /// <summary>
    /// Result object returned from generateContent() call.
    /// </summary>
    public class GenerateContentResult
    {
        public EnhancedGenerateContentResponse? Response { get; set; }
    }

    /// <summary>
    /// Response from calling {@link GenerativeModel.countTokens}.
    /// </summary>
    public class CountTokensResponse
    {
        public int TotalTokens { get; set; }
    }

    /// <summary>
    /// Response from calling {@link GenerativeModel.batchEmbedContents}.
    /// </summary>
    public class BatchEmbedContentsResponse
    {
        public ContentEmbedding[] Embeddings { get; set; }
    }

    /// <summary>
    /// Response from calling {@link GenerativeModel.embedContent}.
    /// </summary>
    public class EmbedContentResponse
    {
        public ContentEmbedding Embedding { get; set; }
    }

    /// <summary>
    /// A single content embedding.
    /// </summary>
    public class ContentEmbedding
    {
        public int[] Values { get; set; }
    }

    /// <summary>
    /// Response object wrapped with helper methods.
    /// </summary>
    public class EnhancedGenerateContentResponse: GenerateContentResponse
    {
        public virtual string? Text()
        {
            return this.Candidates?[0].Content?.Parts?[0].Text;
        }

        public ChatFunctionCall? GetFunction()
        {
            return Candidates?[0].Content?.Parts?[0].FunctionCall;
        }
    }

    /// <summary>
    /// Individual response from {@link GenerativeModel.generateContent} and
    /// {@link GenerativeModel.generateContentStream}.
    /// `generateContentStream()` will return one in each chunk until
    /// the stream is done.
    /// </summary>
    public class GenerateContentResponse
    {
        public GenerateContentCandidate[]? Candidates { get; set; }
        public PromptFeedback? PromptFeedback { get; set; }
    }
    /// <summary>
    /// A candidate returned as part of a {@link GenerateContentResponse}.
    /// </summary>
    public class GenerateContentCandidate
    {
        public int? Index { get; set; }
        public Content? Content { get; set; }
        public FinishReason FinishReason { get; set; }
        public string FinishMessage { get; set; }
        public SafetyRating[]? SafetyRatings { get; set; }
        public CitationMetadata? CitationMetadata { get; set; }
    }
    /// <summary>
    /// If the prompt was blocked, this will be populated with `blockReason` and
    /// the relevant `safetyRatings`.
    /// </summary>
    public class PromptFeedback
    {
        public BlockReason BlockReason { get; set; }
        public SafetyRating[] SafetyRatings { get; set; }
        public string? BlockReasonMessage { get; set; }
    }

    /// <summary>
    /// A safety rating associated with a {@link GenerateContentCandidate}
    /// </summary>
    public class SafetyRating
    {
        public HarmCategory Category { get; set; }
        public HarmProbability Probability { get; set; }
    }

    /// <summary>
    /// Citation metadata that may be found on a {@link GenerateContentCandidate}.
    /// </summary>
    public class CitationMetadata
    {
        public CitationSource[]? CitationSources { get; set; }
    }

    /// <summary>
    /// A single citation source.
    /// </summary>
    public class CitationSource
    {
        public int? StartIndex { get; set; }
        public int? EndIndex { get; set;}
        public string? Uri { get; set; }
        public string? License { get; set; }
    }
}
