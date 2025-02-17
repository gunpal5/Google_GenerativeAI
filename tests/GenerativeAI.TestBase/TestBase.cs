
using GenerativeAI.Core;
using Xunit;

namespace GenerativeAI.Tests;

public abstract class TestBase
{
    public const string SemanticTestsDisabledMessage = "Semantic tests disabled. Add Environment variable 'SEMANTIC_TESTS_ENABLED=true' with proper ADC configuration to run these tests.";
    public const string GeminiTestSkipMessage = "Gemini tests skipped. Add Environment variable 'GEMINI_API_KEY' to run these tests.";
    public const string VertextTestSkipMesaage = "VertexAI tests skipped. Add Environment variable 'VERTEXT_AI_TESTS_ENABLED=true' with proper ADC configurations to run these tests.";

    public static bool IsSemanticTestsEnabled
    {
        get
        {
            return Environment.GetEnvironmentVariable("SEMANTIC_TESTS_ENABLED")?.ToLower() == "true" && IsAdcConfigured;
        }
    }

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

    public static bool IsGeminiApiKeySet
    {
        get
        {
            return !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("GEMINI_API_KEY"));
        }
    }
    
    public static bool IsGoogleApiKeySet
    {
        get
        {
            return !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("GOOGLE_API_KEY"));
        }
    }
    public static bool SkipVertexAITests
    {
        get
        {
            return Environment.GetEnvironmentVariable("VERTEXT_AI_TESTS_ENABLED")?.ToLower()!="true" && !IsAdcConfigured;
        }
    }

    public static bool IsGitHubEnvironment
    {
        get
        {
            return Environment.GetEnvironmentVariable("IsGitHubEnvironment")?.ToLower() == "true";
        }
    }
    protected ITestOutputHelper Console { get; }
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
    
    protected  IPlatformAdapter GetTestVertexAIPlatform()
    {
        var apiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY", EnvironmentVariableTarget.User);
        var projectId = Environment.GetEnvironmentVariable("GOOGLE_PROJECT_ID", EnvironmentVariableTarget.User);
        var region = Environment.GetEnvironmentVariable("GOOGLE_REGION", EnvironmentVariableTarget.User);

        return new VertextPlatformAdapter(projectId, region,false, apiKey);
    }

    protected bool GitHubEnvironment()
    {
        return IsGitHubEnvironment;
    }
}