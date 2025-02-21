namespace GenerativeAI;

/// <summary>
/// Provides constant URLs used as base endpoints for various AI platform services in the GenerativeAI namespace.
/// This class contains static members that define the foundational endpoints required for API calls
/// in services implementing generative and AI capabilities.
/// </summary>
public static class BaseUrls
{
    /// <summary>
    /// Represents the base URL for accessing the Vertex AI platform's API.
    /// This constant is used to construct endpoint URLs for interacting with Google Cloud's Vertex AI services.
    /// The URL includes placeholders for region, API version, project ID, and other parameters required to define
    /// the specific service endpoint within the platform.
    /// </summary>
    public const string VertexAI = "https://{region}-aiplatform.googleapis.com/{version}/projects/{projectId}/locations/{region}";

    /// <summary>
    /// Represents the base URL for accessing the Google Generative AI API.
    /// This constant is used to construct endpoint URLs for interacting with Google's generative language model services.
    /// The URL enables integration with AI-powered generative functionalities within applications.
    /// </summary>
    public const string GoogleGenerativeAI = "https://generativelanguage.googleapis.com";

    /// <summary>
    /// Represents the express base URL for accessing Google Cloud's Vertex AI platform.
    /// This constant provides a simplified endpoint URL for utilizing Vertex AI services without requiring regional
    /// or project-specific details, facilitating more streamlined and general-purpose API interactions.
    /// </summary>
    public const string VertexAIExpress = "https://aiplatform.googleapis.com";

    public const string GoogleMultiModalLive =
        "wss://generativelanguage.googleapis.com/ws/google.ai.generativelanguage.{version}.GenerativeService.BidiGenerateContent";
    public const string VertexMultiModalLiveGlobal =
        "wss://aiplatform.googleapis.com/ws/google.cloud.aiplatform.{version}.LlmBidiService/BidiGenerateContent";
    public const string VertexMultiModalLive =
        "wss://{location}-aiplatform.googleapis.com/ws/google.cloud.aiplatform.{version}.LlmBidiService/BidiGenerateContent";
}