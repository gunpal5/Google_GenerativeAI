using GenerativeAI.Authenticators;
using GenerativeAI.Core;
using Xunit;

namespace GenerativeAI.Tests;

public abstract class TestBase
{
    /// <summary>
    /// Message displayed when semantic tests are disabled.
    /// </summary>
    public const string SemanticTestsDisabledMessage =
        "Semantic tests disabled. Add Environment variable 'SEMANTIC_TESTS_ENABLED=true' with proper ADC configuration to run these tests.";

    /// <summary>
    /// Message displayed when Gemini tests are skipped.
    /// </summary>
    public const string GeminiTestSkipMessage =
        "Gemini tests skipped. Add Environment variable 'GEMINI_API_KEY' to run these tests.";

    /// <summary>
    /// Message displayed when Vertex AI tests are skipped.
    /// </summary>
    public const string VertextTestSkipMesaage =
        "VertexAI tests skipped. Add Environment variable 'VERTEXT_AI_TESTS_ENABLED=true' with proper ADC configurations to run these tests.";

    /// <summary>
    /// Determines if semantic tests are enabled based on the SEMANTIC_TESTS_ENABLED environment variable
    /// and whether ADC (Application Default Credentials) is properly configured.
    /// </summary>
    public static bool IsSemanticTestsEnabled
    {
        get
        {
            return Environment.GetEnvironmentVariable("SEMANTIC_TESTS_ENABLED")?.ToLower() == "true" && IsAdcConfigured;
        }
    }

    /// <summary>
    /// Checks if Application Default Credentials (ADC) are configured, looking for credentials
    /// in various environment variables or default file paths.
    /// </summary>
    public static bool IsAdcConfigured
    {
        get
        {
            var credentialsFile = EnvironmentVariables.GOOGLE_WEB_CREDENTIALS;

            if (string.IsNullOrEmpty(credentialsFile))
            {
                credentialsFile = EnvironmentVariables.GOOGLE_APPLICATION_CREDENTIALS;
            }

            if (string.IsNullOrEmpty(credentialsFile))
            {
                credentialsFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "gcloud",
                    "application_default_credentials.json");
            }

            return !string.IsNullOrEmpty(credentialsFile) && File.Exists(credentialsFile);
        }
    }

    /// <summary>
    /// Checks if the Gemini API key is set in the environment variables.
    /// </summary>
    public static bool IsGeminiApiKeySet
    {
        get { return !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("GEMINI_API_KEY")); }
    }

    /// <summary>
    /// Checks if the Google API key is set in the environment variables.
    /// </summary>
    public static bool IsGoogleApiKeySet
    {
        get { return !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("GOOGLE_API_KEY")); }
    }

    /// <summary>
    /// Determines if Vertex AI tests should be skipped, based on the VERTEXT_AI_TESTS_ENABLED environment variable
    /// and ADC (Application Default Credentials) configuration.
    /// </summary>
    public static bool SkipVertexAITests
    {
        get
        {
            return Environment.GetEnvironmentVariable("VERTEXT_AI_TESTS_ENABLED")?.ToLower() != "true" ||
                   !IsAdcConfigured;
        }
    }

    /// <summary>
    /// Checks if GitHub environment is active, based on the IsGitHubEnvironment environment variable.
    /// </summary>
    public static bool IsGitHubEnvironment
    {
        get { return Environment.GetEnvironmentVariable("IsGitHubEnvironment")?.ToLower() == "true"; }
    }

    protected ITestOutputHelper? Console { get; }

    protected TestBase()
    {
    }

    protected TestBase(ITestOutputHelper testOutputHelper)
    {
        this.Console = testOutputHelper;
    }

    protected virtual IPlatformAdapter GetTestGooglePlatform()
    {
        //return GetTestVertexAIPlatform();
        var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY", EnvironmentVariableTarget.User);

        return new GoogleAIPlatformAdapter(apiKey);
    }

    protected virtual IPlatformAdapter GetTestVertexAIPlatform()
    {
        var apiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY", EnvironmentVariableTarget.User);
        var projectId = Environment.GetEnvironmentVariable("GOOGLE_PROJECT_ID", EnvironmentVariableTarget.User);
        var region = Environment.GetEnvironmentVariable("GOOGLE_REGION", EnvironmentVariableTarget.User);

        return new VertextPlatformAdapter(projectId, region, false, apiKey);
    }

    protected bool GitHubEnvironment()
    {
        return IsGitHubEnvironment;
    }
}