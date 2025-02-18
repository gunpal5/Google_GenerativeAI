namespace GenerativeAI;

/// <summary>
/// The GenerativeModelTasks class defines a set of constant task names used to interact with generative AI models.
/// These tasks are used across various operations for processing and generating content, embeddings,
/// and token analysis.
/// </summary>
/// <remarks>
/// The constants in this class represent specific actions that can be performed using generative AI models.
/// They are typically utilized in constructing API endpoints or specifying desired operations.
/// </remarks>
public static class GenerativeModelTasks
{
    /// <summary>
    /// Task used to generate content using a generative AI model.
    /// </summary>
    public const string GenerateContent = "generateContent";

    /// <summary>
    /// Task used to stream content generation from a generative AI model.
    /// </summary>
    public const string StreamGenerateContent = "streamGenerateContent";

    /// <summary>
    /// Task used to count the number of tokens in a given input.
    /// </summary>
    public const string CountTokens = "countTokens";

    /// <summary>
    /// Task used to generate an embedding for a single piece of content.
    /// </summary>
    public const string EmbedContent = "embedContent";

    /// <summary>
    /// Task used to generate embeddings for multiple pieces of content in a batch.
    /// </summary>
    public const string BatchEmbedContents = "batchEmbedContents";

    /// <summary>
    /// Task used to generate a textual answer to a given question or query.
    /// </summary>
    public const string GenerateAnswer = "generateAnswer";
}