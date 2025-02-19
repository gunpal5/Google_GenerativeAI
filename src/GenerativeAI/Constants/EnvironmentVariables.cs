namespace GenerativeAI;

/// <summary>
/// The EnvironmentVariables class provides access to various environment variables used for configuring
/// Google AI services. These variables store information such as project ID, region, API keys,
/// and authentication credentials. Default values are used for certain variables if they are not set.
/// </summary>
public static class EnvironmentVariables
{
    /// <summary>
    /// Represents the Google Cloud Project ID for the application.
    /// This variable is sourced from the "GOOGLE_PROJECT_ID" environment variable.
    /// It is used to identify the specific Google Cloud project being utilized
    /// for services such as AI models, storage, or other resources.
    /// If the environment variable is not set, this value may be null.
    /// </summary>
    public static readonly string? GOOGLE_PROJECT_ID = Environment.GetEnvironmentVariable("GOOGLE_PROJECT_ID");

    /// <summary>
    /// Represents the Google Cloud region used for the application.
    /// This variable is derived from the "GOOGLE_REGION" environment variable.
    /// It specifies the geographical location where Google Cloud resources
    /// and services are to be utilized, typically enhancing performance and
    /// compliance based on locality.
    /// If the environment variable is not set, the default value
    /// is "us-central1".
    /// </summary>
    public static readonly string GOOGLE_REGION = Environment.GetEnvironmentVariable("GOOGLE_REGION") ?? "us-central1";

    /// <summary>
    /// Represents the Google Access Token used for authenticating API requests to Google services.
    /// This variable is sourced from the "GOOGLE_ACCESS_TOKEN" environment variable.
    /// It provides direct authorization for making requests without requiring additional credentials or API keys.
    /// If the environment variable is not set, this value may be null.
    /// </summary>
    public static readonly string? GOOGLE_ACCESS_TOKEN = Environment.GetEnvironmentVariable("GOOGLE_ACCESS_TOKEN");

    /// <summary>
    /// Represents the Google API Key used for authenticating requests to Google services.
    /// This variable is sourced from the "GOOGLE_API_KEY" environment variable.
    /// It is required for accessing Google APIs, including AI and cloud platform features.
    /// If the environment variable is not set, this value may be null.
    /// </summary>
    public static readonly string? GOOGLE_API_KEY = Environment.GetEnvironmentVariable("GOOGLE_API_KEY");

    /// <summary>
    /// Specifies the Google AI Model to be used by the application.
    /// This variable is sourced from the "GOOGLE_AI_MODEL" environment variable,
    /// or defaults to a predefined model (DefaultGeminiModel) if the variable is not set.
    /// It is utilized by services interfacing with Google's generative AI capabilities,
    /// such as Vertex AI or other AI-related APIs.
    /// </summary>
    public static readonly string? GOOGLE_AI_MODEL = Environment.GetEnvironmentVariable("GOOGLE_AI_MODEL") ?? GoogleAIModels.DefaultGeminiModel;

    /// <summary>
    /// Specifies the file path to the Google Cloud service account key file.
    /// This variable is sourced from the "GOOGLE_APPLICATION_CREDENTIALS" environment variable.
    /// It is required for authenticating API calls to Google Cloud services when using
    /// service account credentials. If the environment variable is not set, this value may be null.
    /// </summary>
    public static readonly string? GOOGLE_APPLICATION_CREDENTIALS = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");

    /// <summary>
    /// Represents the path or content of the Google OAuth2 web credentials file for the application.
    /// This variable is sourced from the "GOOGLE_WEB_CREDENTIALS" environment variable.
    /// It is utilized for authenticating requests to Google Cloud services that require OAuth2-based credentials.
    /// If the environment variable is not set, this value may be null, potentially resulting in authentication failures.
    /// </summary>
    public static readonly string? GOOGLE_WEB_CREDENTIALS = Environment.GetEnvironmentVariable("GOOGLE_WEB_CREDENTIALS");
}