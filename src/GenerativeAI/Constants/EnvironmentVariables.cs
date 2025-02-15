namespace GenerativeAI;

public class EnvironmentVariables
{
    public static readonly string? GOOGLE_PROJECT_ID = Environment.GetEnvironmentVariable("GOOGLE_PROJECT_ID");
    public static readonly string? GOOGLE_REGION = Environment.GetEnvironmentVariable("GOOGLE_REGION") ?? "us-central1"; 
    public static readonly string? GOOGLE_ACCESS_TOKEN = Environment.GetEnvironmentVariable("GOOGLE_ACCESS_TOKEN");
    public static readonly string? GOOGLE_API_KEY = Environment.GetEnvironmentVariable("GOOGLE_API_KEY");
    public static readonly string? GOOGLE_AI_MODEL = Environment.GetEnvironmentVariable("GOOGLE_AI_MODEL") ?? GoogleAIModels.DefaultGeminiModel; 
    public static readonly string? GOOGLE_APPLICATION_CREDENTIALS = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
    public static readonly string? GOOGLE_WEB_CREDENTIALS = Environment.GetEnvironmentVariable("GOOGLE_WEB_CREDENTIALS");
}