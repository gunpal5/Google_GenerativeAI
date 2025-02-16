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
public class GenerativeModelTasks
{
    public const string GenerateContent = "generateContent";
    public const string StreamGenerateContent = "streamGenerateContent";
    public const string CountTokens = "countTokens";
    public const string EmbedContent = "embedContent";
    public const string BatchEmbedContents = "batchEmbedContents";
    public const string GenerateAnswer = "generateAnswer";
}