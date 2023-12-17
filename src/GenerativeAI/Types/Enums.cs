namespace GenerativeAI.Types
{
    /// <summary>
    /// Harm categories that would cause prompts or candidates to be blocked.
    /// </summary>
    public enum HarmCategory
    {
        HARM_CATEGORY_UNSPECIFIED,
        HARM_CATEGORY_HATE_SPEECH,
        HARM_CATEGORY_SEXUALLY_EXPLICIT,
        HARM_CATEGORY_HARASSMENT,
        HARM_CATEGORY_DANGEROUS_CONTENT,
    }

    /// <summary>
    /// Reason that a prompt was blocked.
    /// </summary>
    public enum BlockReason
    {
        /// <summary>
        /// A blocked reason was not specified.
        /// </summary>
        BLOCKED_REASON_UNSPECIFIED,
        /// <summary>
        /// Content was blocked by safety settings.
        /// </summary>
        SAFETY,
        /// <summary>
        /// Content was blocked, but the reason is uncategorized.
        /// </summary>
        OTHER,
    }
    /// <summary>
    /// Threshhold above which a prompt or candidate will be blocked.
    /// </summary>
    public enum HarmBlockThreshold
    {
        /// <summary>
        /// Threshold is unspecified.
        /// </summary>
        HARM_BLOCK_THRESHOLD_UNSPECIFIED,
        /// <summary>
        /// Content with NEGLIGIBLE will be allowed.
        /// </summary>
        BLOCK_LOW_AND_ABOVE,
        /// <summary>
        /// Content with NEGLIGIBLE and LOW will be allowed.
        /// </summary>
        BLOCK_MEDIUM_AND_ABOVE,
        /// <summary>
        /// Content with NEGLIGIBLE, LOW, and MEDIUM will be allowed.
        /// </summary>
        BLOCK_ONLY_HIGH,
        /// <summary>
        /// All content will be allowed.
        /// </summary>
        BLOCK_NONE,
    }

    /// <summary>
    /// Probability that a prompt or candidate matches a harm category.
    /// </summary>
    public enum HarmProbability
    {
        /// <summary>
        /// Probability is unspecified.
        /// </summary>
        HARM_PROBABILITY_UNSPECIFIED,
        // Content has a negligible chance of being unsafe.
        NEGLIGIBLE,
        /// <summary>
        /// Content has a low chance of being unsafe.
        /// </summary>
        LOW,
        /// <summary>
        /// Content has a medium chance of being unsafe.
        /// </summary>
        MEDIUM,
        /// <summary>
        /// Content has a high chance of being unsafe.
        /// </summary>
        HIGH
    }

    /// <summary>
    /// Reason that a candidate finished.
    /// </summary>
    public enum FinishReason
    {
        /// <summary>
        /// Default value. This value is unused.
        /// </summary>
        FINISH_REASON_UNSPECIFIED,
        /// <summary>
        /// Natural stop point of the model or provided stop sequence.
        /// </summary>
        STOP,
        /// <summary>
        /// The maximum number of tokens as specified in the request was reached.
        /// </summary>
        MAX_TOKENS,
        /// <summary>
        /// The candidate content was flagged for safety reasons.
        /// </summary>
        SAFETY,
        /// <summary>
        /// The candidate content was flagged for recitation reasons.
        /// </summary>
        RECITATION,
        /// <summary>
        /// Unknown reason.
        /// </summary>
        OTHER,
    }

    /// <summary>
    /// Task type for embedding content.
    /// </summary>
    public enum TaskType
    {
        TASK_TYPE_UNSPECIFIED,
        RETRIEVAL_QUERY,
        RETRIEVAL_DOCUMENT,
        SEMANTIC_SIMILARITY,
        CLASSIFICATION,
        CLUSTERING,
    }
}
